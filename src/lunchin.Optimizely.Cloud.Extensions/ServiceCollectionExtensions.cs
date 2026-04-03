using EPiServer.Applications;
using EPiServer.DataAbstraction.RuntimeModel;
using EPiServer.DependencyInjection;
using EPiServer.Shell;
using lunchin.Optimizely.Cloud.Extensions.Settings;
using lunchin.Optimizely.Cloud.Extensions.Taxonomy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;

namespace lunchin.Optimizely.Cloud.Extensions;

public static class ServiceCollectionExtensions
{
    private const string _moduleName = "lunchin.Optimizely.Cloud.Extensions";

    public static IServiceCollection AddlunchinOptimizelyCloudExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(StorageExplorerController).Assembly);

        services.AddSingleton<IStorageService, StorageService>()
         .AddSingleton<ISettingsService, SettingsService>()
         .AddSingleton<IContentTypeBaseProvider, SettingsContentTypeBaseProvider>()
         .AddSingleton<IContentTypeBaseProvider, ClassificationContentTypeBaseResolver>()
         .AddSingleton<IContentRepositoryDescriptor, ClassificationContentRepositoryDescriptor>()
         .Configure<ExtensionsOptions>(configuration.GetSection(ExtensionsOptions.Path))
         .Configure<AuthorizationOptions>(x => x.TryAddPolicy("loce:policy", p => p.RequireRole(Roles.Administrators, Roles.CmsAdmins)))
         .Configure<ProtectedModuleOptions>(moduleOptions =>
         {
             if (!moduleOptions.Items.Any(item => item.Name.Equals(_moduleName, StringComparison.OrdinalIgnoreCase)))
             {
                 moduleOptions.Items.Add(new ModuleDetails { Name = _moduleName });
             }
         })
         .AddCmsEvents()
         .AddCmsEventType<SettingEventData>()
         .AddCmsEventSubscriber<ApplicationCreatedEvent, ISettingsService>()
         .AddCmsEventSubscriber<ApplicationUpdatedEvent, ISettingsService>()
         .AddCmsEventSubscriber<ApplicationDeletedEvent, ISettingsService>()
         .AddCmsEventSubscriber<SettingEventData, ISettingsService>()
         .PostConfigure<StaticFileOptions>(options => options.ContentTypeProvider ??= new FileExtensionContentTypeProvider());

        return services;
    }
}
