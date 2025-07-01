using EPiServer.DataAbstraction.RuntimeModel;
using EPiServer.Shell;
using lunchin.Optimizely.Cloud.Extensions.Settings;
using lunchin.Optimizely.Cloud.Extensions.Taxonomy;

namespace lunchin.Optimizely.Cloud.Extensions;

public static class ServiceCollectionExtensions
{
    private const string ModuleName = "lunchin.Optimizely.Cloud.Extensions";

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
             if (!moduleOptions.Items.Any(item => item.Name.Equals(ModuleName, StringComparison.OrdinalIgnoreCase)))
             {
                 moduleOptions.Items.Add(new ModuleDetails { Name = ModuleName });
             }
         });
        return services;
    }
}
