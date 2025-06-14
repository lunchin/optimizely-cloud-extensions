using EPiServer.Commerce.Internal.Migration;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Web.Mvc;
using sample.Business.Rendering;

namespace sample.Business.Initialization;

/// <summary>
/// Module for customizing templates and rendering.
/// </summary>
[ModuleDependency(typeof(InitializationModule))]
public class CustomizedRenderingInitialization : IConfigurableModule
{
    public void ConfigureContainer(ServiceConfigurationContext context)
    {
        // Implementations for custom interfaces can be registered here.
        context.ConfigurationComplete += (o, e) =>
            // Register custom implementations that should be used in favour of the default implementations
            context.Services.AddTransient<IContentRenderer, ErrorHandlingContentRenderer>()
                .AddSingleton<AlloyContentAreaItemRenderer, AlloyContentAreaItemRenderer>();
    }

    public void Initialize(InitializationEngine context)
    {
        var manager = context.Locate.Advanced.GetInstance<MigrationManager>();
        if (manager.SiteNeedsToBeMigrated())
        {
            manager.Migrate();
        }
        context.Locate.Advanced.GetInstance<ITemplateResolverEvents>().TemplateResolved += TemplateCoordinator.OnTemplateResolved;
        var contentTypeRepository = context.Locate.Advanced.GetInstance<IContentTypeRepository>();
        var availableSettingsRepository = context.Locate.Advanced.GetInstance<IAvailableSettingsRepository>();

        var sysRoot = contentTypeRepository.Load("SysRoot") as PageType;
        var setting = new AvailableSetting { Availability = Availability.All };
        availableSettingsRepository.RegisterSetting(sysRoot, setting);
    }

    public void Uninitialize(InitializationEngine context) =>
        context.Locate.Advanced.GetInstance<ITemplateResolverEvents>().TemplateResolved -= TemplateCoordinator.OnTemplateResolved;
}
