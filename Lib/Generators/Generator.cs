using Dto2Mvc.Lib.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Dto2Mvc.Lib.Generators;

internal static class Generator
{
    internal static void GenerateControllersAndViews<TControllerBase>(this Type dtoType, string webAppOutputPath)
        where TControllerBase : Controller
    {
        foreach (var attribute in dtoType.GetCustomAttributes<Dto2MvcAttribute>(true))
        {
            // Controller generator
            var controllerName = $"{attribute.Controller}Controller";
            var controllerCode = GenerateControllerCode(dtoType, controllerName);

            var ctrlFilePath = Path.Combine(webAppOutputPath, "Controllers", $"{controllerName}.cs");
            Save(ctrlFilePath, controllerCode);

            // View generator
            var viewName = attribute.Action;
            var viewCode = GenerateViewCode(dtoType, viewName);

            var viewFilePath = Path.Combine(webAppOutputPath, "Views", attribute.Controller, $"{viewName}.cshtml");
            Save(viewFilePath, viewCode);
        }
    }

    private static void Save(string filepath, string content)
    {
        CheckOrCreateDirectories(filepath);
        File.WriteAllText(filepath, content);
    }

    private static void CheckOrCreateDirectories(string filepath)
    {
        var directoryPath = Path.GetDirectoryName(filepath)!;
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    private static string GenerateControllerCode(Type dtoType, string controllerName)
    {
        var codeNamespace = new CodeNamespace("GeneratedControllers");
        codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
        codeNamespace.Imports.Add(new CodeNamespaceImport("System.Web.Mvc"));

        var controllerType = new CodeTypeDeclaration(controllerName)
        {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public
        };

        var controllerBaseType = new CodeTypeReference(typeof(Controller));
        controllerType.BaseTypes.Add(controllerBaseType);

        // Generování atributu [HttpGet] nad akcí
        var getMethodAttributes = new CodeAttributeDeclarationCollection
        {
            new CodeAttributeDeclaration(new CodeTypeReference(typeof(HttpGetAttribute)))
        };

        // Generování akce s parametrem
        var actionMethod = new CodeMemberMethod
        {
            Name = "Index",
            Attributes = MemberAttributes.Public | MemberAttributes.Final,
            ReturnType = new CodeTypeReference(typeof(ActionResult))
        };

        actionMethod.CustomAttributes.AddRange(getMethodAttributes);
        actionMethod.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(typeof(ViewResult))));
        controllerType.Members.Add(actionMethod);

        codeNamespace.Types.Add(controllerType);

        return GenerateCode(codeNamespace);
    }

    private static string GenerateViewCode(Type dtoType, string viewName)
    {
        var codeNamespace = new CodeNamespace("GeneratedViews");

        var viewClass = new CodeTypeDeclaration(viewName)
        {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public
        };

        codeNamespace.Types.Add(viewClass);

        return GenerateCode(codeNamespace);
    }

    private static string GenerateCode(CodeNamespace codeNamespace)
    {
        var codeProvider = new CSharpCodeProvider();
        var codeGeneratorOptions = new CodeGeneratorOptions { BracingStyle = "C" };

        using (var stringWriter = new StringWriter())
        {
            codeProvider.GenerateCodeFromNamespace(codeNamespace, stringWriter, codeGeneratorOptions);
            return stringWriter.ToString();
        }
    }
}