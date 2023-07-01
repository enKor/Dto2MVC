using Dto2Mvc.Lib.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Dto2Mvc.Lib.Generators;

internal static class Generator
{
    internal static void GenerateControllersAndViews<TControllerBase>(this Type dtoType, string webAppOutputPath, string ns)
        where TControllerBase : Controller
    {
        foreach (var attribute in dtoType.GetCustomAttributes<Dto2MvcAttribute>(true))
        {
            // Controller generator
            var controllerName = $"{attribute.Controller}Controller";
            var controllerCode = GenerateControllerCode<TControllerBase>(dtoType, controllerName, attribute.Action, ns, attribute);

            var ctrlFilePath = Path.Combine(webAppOutputPath, "Controllers", $"{controllerName}.{attribute.Action}.cs");
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

    private static string GenerateControllerCode<TControllerBase>(Type dtoType, 
        string controllerName, string actionName, string ns,
        Dto2MvcAttribute attribute)
        where TControllerBase : Controller
    {
        var codeNamespace = CreateNamespace<TControllerBase>(ns);

        var controllerType = CreateControllerType<TControllerBase>(controllerName);

        var methodAttributes = CreateAttributes(attribute);

        SetActionMethod(methodAttributes, controllerType, actionName);

        codeNamespace.Types.Add(controllerType);

        return GenerateCode(codeNamespace);
    }

    private static CodeNamespace CreateNamespace<TControllerBase>(string ns) where TControllerBase : Controller
    {
        var codeNamespace = new CodeNamespace(ns);
        codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
        codeNamespace.Imports.Add(new CodeNamespaceImport("System.Web.Mvc"));
        return codeNamespace;
    }

    private static CodeTypeDeclaration CreateControllerType<TControllerBase>(string controllerName)
        where TControllerBase : Controller
    {
        var controllerType = new CodeTypeDeclaration(controllerName)
        {
            IsClass = true,
            TypeAttributes = TypeAttributes.Public,
            IsPartial = true
        };

        var controllerBaseType = new CodeTypeReference(typeof(TControllerBase));
        controllerType.BaseTypes.Add(controllerBaseType);
        return controllerType;
    }

    private static void SetActionMethod(CodeAttributeDeclarationCollection methodAttributes,
        CodeTypeDeclaration controllerType, string actionName) 
    {
        var actionMethod = new CodeMemberMethod
        {
            Name = actionName,
            Attributes = MemberAttributes.Public,
            ReturnType = new CodeTypeReference(typeof(ActionResult))
        };

        actionMethod.CustomAttributes.AddRange(methodAttributes);
        actionMethod.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(typeof(ViewResult))));
        controllerType.Members.Add(actionMethod);
    }

    private static CodeAttributeDeclarationCollection CreateAttributes(Dto2MvcAttribute attribute)
    {
        var methodAttribute = attribute.Method switch
        {
            Dto2MvcAttribute.HttpMethod.Get => typeof(HttpGetAttribute),
            Dto2MvcAttribute.HttpMethod.Post => typeof(HttpPostAttribute),
            _ => throw new NotImplementedException()
        };

        var getMethodAttributes = new CodeAttributeDeclarationCollection
        {
            new CodeAttributeDeclaration(new CodeTypeReference(methodAttribute))
        };
        return getMethodAttributes;
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

        using var stringWriter = new StringWriter();
        codeProvider.GenerateCodeFromNamespace(codeNamespace, stringWriter, codeGeneratorOptions);
        return stringWriter.ToString();
    }
}