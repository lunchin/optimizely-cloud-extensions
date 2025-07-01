using EPiServer.Authorization;
using EPiServer.Azure.Blobs;
using EPiServer.Cms.Shell;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Framework.Hosting;
using EPiServer.Framework.Web.Resources;
using EPiServer.Scheduler;
using EPiServer.Web.Hosting;
using EPiServer.Web.Routing;
using lunchin.Optimizely.Cloud.Extensions;
using lunchin.Optimizely.Cloud.Extensions.Commerce;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using sample.Extensions;

namespace sample;

public class Startup
{
    private readonly IWebHostEnvironment _webHostingEnvironment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<Startup> _logger;

    public Startup(IWebHostEnvironment webHostingEnvironment, IConfiguration configuration)
    {
        _webHostingEnvironment = webHostingEnvironment;
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        if (_webHostingEnvironment.IsDevelopment())
        {
            services.Configure<SchedulerOptions>(options => options.Enabled = false);
            services.Configure<ClientResourceOptions>(uiOptions => uiOptions.Debug = true);
            services.Configure<CompositeFileProviderOptions>(c => c.BasePathFileProviders.Add(new MappingPhysicalFileProvider("/EPiServer/lunchin.Optimizely.Cloud.Extensions", string.Empty, Path.Combine(_webHostingEnvironment.ContentRootPath, "..\\src\\lunchin.Optimizely.Cloud.Extensions"))));
            services.Configure<CompositeFileProviderOptions>(c => c.BasePathFileProviders.Add(new MappingPhysicalFileProvider("/EPiServer/lunchin.Optimizely.Cloud.Extensions.Commerce", string.Empty, Path.Combine(_webHostingEnvironment.ContentRootPath, "..\\src\\lunchin.Optimizely.Cloud.Extensions.Commerce"))));
        }

        services
            .AddCmsAspNetIdentity<ApplicationUser>()
            .AddCms()
            .AddCommerce()
            .AddlunchinOptimizelyCloudExtensions(_configuration)
            .AddlunchinOptimizelyCommerceCloudExtensions(_configuration)
            .AddAlloy()
            .AddAdminUserRegistration(options =>
            {
                options.Behavior = EPiServer.Cms.Shell.UI.RegisterAdminUserBehaviors.Enabled | EPiServer.Cms.Shell.UI.RegisterAdminUserBehaviors.LocalRequestsOnly;
                options.Roles = [Roles.Administrators, Roles.WebAdmins];
            })
            .AddEmbeddedLocalization<Startup>();

        services.Configure<AzureBlobProviderOptions>(o =>
        {
            o.ConnectionString =  _configuration.GetConnectionString("AzureBlobConnection");
        });
        //services.Configure<ExtensionsOptions>(options => options.HideDefaultCategoryEnabled = false);

        // Required by Wangkanai.Detection
        services.AddDetection();

        services.AddSession(options =>
        {
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

        app.UselunchinOptimizelyCloudExtensions();
        app.UselunchinOptimizelyCommerceCloudExtensions();
        // Required by Wangkanai.Detection
        app.UseDetection();
        app.UseSession();

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => endpoints.MapContent());
    }
}
