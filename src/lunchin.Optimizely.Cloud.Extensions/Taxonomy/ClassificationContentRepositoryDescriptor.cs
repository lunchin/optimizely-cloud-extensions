using EPiServer.Cms.Shell.UI.CompositeViews.Internal;
using EPiServer.Shell;

namespace lunchin.Optimizely.Cloud.Extensions.Taxonomy;

public class ClassificationContentRepositoryDescriptor : ContentRepositoryDescriptorBase
{
    public const string RepositoryKey = "classifications";

    public override string Key => RepositoryKey;

    public override string Name => LocalizationService.Current.GetString("/perficient/taxonomy/title", "Classifications");

    public virtual string CreatingTypeIdentifier => (typeof(ClassificationData))?.FullName?.ToLowerInvariant() ?? "lunchin.Optimizely.Cloud.Extensions.Taxonomy.ClassificationData";

    public override IEnumerable<Type> CreatableTypes => [typeof(ClassificationData), typeof(ClassificationFolder)];

    public override IEnumerable<Type> ContainedTypes => [typeof(ClassificationData), typeof(ClassificationFolder)];

    public override IEnumerable<Type> LinkableTypes => [typeof(ClassificationData)];

    public override IEnumerable<ContentReference> Roots => [ClassificationFolder.ClassificationRoot];

    public override IEnumerable<Type> MainNavigationTypes => [typeof(ClassificationData), typeof(ClassificationFolder)];

    public override string CustomNavigationWidget => "epi-cms/component/ContentNavigationTree";

    public override IEnumerable<string> MainViews => [HomeView.ViewName];

    public override string SearchArea => ClassifiicationSearchProvider.SearchArea;

    public override int SortOrder => 900;
}
