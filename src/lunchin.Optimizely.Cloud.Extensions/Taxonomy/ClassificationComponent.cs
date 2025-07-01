using EPiServer.Shell;
using EPiServer.Shell.ViewComposition;

namespace lunchin.Optimizely.Cloud.Extensions.Taxonomy;

[Component]
public sealed class ClassificationComponent : ComponentDefinitionBase
{
    public ClassificationComponent()
        : base("epi-cms/component/MainNavigationComponent")
    {
        LanguagePath = "/episerver/cms/components/taxonomy";
        Title = "Taxonomy";
        SortOrder = 1000;
        PlugInAreas = [PlugInArea.AssetsDefaultGroup];
        Settings.Add(new Setting("repositoryKey", value: ClassificationContentRepositoryDescriptor.RepositoryKey));
    }
}
