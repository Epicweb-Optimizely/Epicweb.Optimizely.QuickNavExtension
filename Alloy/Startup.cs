using Epicweb.Alloy.QuickNavExtension.Extensions;
using EPiServer.Cms.Shell;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using EPiServer.Find;
using EPiServer.Find.Cms;
using Epicweb.Optimizely.QuickNavExtension;
using EPiServer.Cms.Shell.UI.Attributes;
using EPiServer.Shell.Security;
using Microsoft.AspNetCore.Mvc;

namespace Epicweb.Alloy.QuickNavExtension;

public class Startup
{
    private readonly IWebHostEnvironment _webHostingEnvironment;

    public Startup(IWebHostEnvironment webHostingEnvironment)
    {
        _webHostingEnvironment = webHostingEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        if (_webHostingEnvironment.IsDevelopment())
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(_webHostingEnvironment.ContentRootPath, "App_Data"));

            services.Configure<SchedulerOptions>(options => options.Enabled = false);
        }

        services
            .AddCmsAspNetIdentity<ApplicationUser>()
            .AddCms()
            .AddAlloy()
            .AddAdminUserRegistration()
            .AddEmbeddedLocalization<Startup>()
            .AddFind();
        //.AddFindCore();

        //add links in a simple way in startup. 
        services
            .AddQuickNav("Custom link", "https://devblog.gosso.se/", role: "WebDevs")
            .AddQuickNav("Custom Javascript", "javascript:if(confirm(\'R U SURE?\')){document.location=\'/\';}", role: "WebDevs")
            .AddQuickNav(QuickNavigator.FIND)
            .AddQuickNav(QuickNavigator.Admin)
            .AddQuickNav(QuickNavigator.ContentType)
            //.AddQuickNav("wrong input")
            .AddQuickNav(QuickNavigator.LogOut);


        // Required by Wangkanai.Detection
        services.AddDetection();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Required by Wangkanai.Detection
        app.UseDetection();
        app.UseSession();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();//add this to make [Route("")] work
            endpoints.MapContent();
        });
    }
}
