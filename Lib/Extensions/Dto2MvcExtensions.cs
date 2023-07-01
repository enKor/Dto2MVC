using Dto2Mvc.Lib.Attributes;
using Dto2Mvc.Lib.Generators;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;
using System.Reflection;

namespace Dto2Mvc.Lib.Extensions;

public static class Dto2MvcExtensions
{
    public static void AddDto2Mvc<TControllerBase>(string webAppOutputPath, params Type[] pivots)
        where TControllerBase : Controller
    {
        var types = pivots
            .SelectMany(t => t.Assembly.GetTypes())
            .Distinct()
            .Where(t => t.GetCustomAttributes<Dto2MvcAttribute>().Any())
            .ToImmutableList();

        foreach (var t in types)
        {
            t.GenerateControllersAndViews<TControllerBase>(webAppOutputPath);
        }
    }
}