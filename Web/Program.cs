using Dto2Mvc.Lib.Extensions;
using Web.Controllers;
using Web.Models;

namespace Web;

public class Program
{
    public static void Main(string[] args)
    {
        Dto2MvcExtensions.AddDto2Mvc<MyControllerBase>(
            Environment.CurrentDirectory, 
            typeof(HomeController).Namespace!,
            typeof(CarModel));

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}