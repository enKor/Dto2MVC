using Dto2Mvc.Lib.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Reflection;

namespace Dto2Mvc.Lib.Extensions;

public static class Dto2MvcExtensions
{
    public static void AddDto2Mvc<TController>(string webAppOutputPath, params Type[] pivots)
        where TController : Controller
    {
        var types = pivots
            .SelectMany(t => t.Assembly.GetTypes())
            .Distinct()
            .Where(t => t.GetCustomAttributes<Dto2MvcAttribute>().Any())
            .ToImmutableList();

        foreach (var t in types)
        {
            t.GenerateControllerAndView(webAppOutputPath);
        }
    }

    public static void GenerateControllerAndView(this Type dtoType, string webAppOutputPath)
    {
        // Generování Controlleru
        var controllerName = $"{dtoType.Name}Controller";
        var controllerCode = GenerateControllerCode(dtoType, controllerName);

        var controllerFilePath = Path.Combine(webAppOutputPath, $"{controllerName}.cs");
        File.WriteAllText(controllerFilePath, controllerCode);

        // Generování View
        var viewName = $"{dtoType.Name}View";
        var viewCode = GenerateViewCode(dtoType, viewName);

        var viewFilePath = Path.Combine(webAppOutputPath, $"{viewName}.cshtml");
        File.WriteAllText(viewFilePath, viewCode);
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