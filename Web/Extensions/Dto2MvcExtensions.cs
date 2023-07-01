using Microsoft.AspNetCore.Mvc;

namespace Web.Extensions;

public static class Dto2MvcExtensions
{
    public static IServiceCollection AddDto2Mvc<TController>(this IServiceCollection services,
        params Type[] pivots)
    where TController: Controller
    {


        return services;
    }
}