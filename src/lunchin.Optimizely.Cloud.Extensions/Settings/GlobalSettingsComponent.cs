using EPiServer.Shell;
using EPiServer.Shell.ViewComposition;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions.Settings;

[Component]
public sealed class GlobalSettingsComponent : ComponentDefinitionBase, IPluggableComponentDefinition
{
    public GlobalSettingsComponent()
        : base("epi-cms/component/MainNavigationComponent")
    {
        LanguagePath = "/episerver/cms/components/globalsettings";
        Title = "Site settings";
        SortOrder = 1000;

        Settings.Add(new Setting("repositoryKey", value: GlobalSettingsRepositoryDescriptor.RepositoryKey));
        var options = ServiceLocator.Current.GetInstance<IOptions<ExtensionsOptions>>().Value;
        if (options?.SettingsEnabled ?? false)
        {
            PlugInAreas = [PlugInArea.AssetsDefaultGroup];
        }
    }
}