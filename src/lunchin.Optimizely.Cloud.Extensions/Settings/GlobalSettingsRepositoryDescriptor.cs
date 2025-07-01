using EPiServer.Cms.Shell.UI.CompositeViews.Internal;
using EPiServer.Shell;

namespace lunchin.Optimizely.Cloud.Extensions.Settings;

[ServiceConfiguration(typeof(IContentRepositoryDescriptor))]
public class GlobalSettingsRepositoryDescriptor : ContentRepositoryDescriptorBase
{
    public static string RepositoryKey => "globalsettings";

    public override IEnumerable<Type> ContainedTypes => [
        typeof(SettingsBase),
        typeof(SettingsFolder)
    ];

    public override IEnumerable<Type> CreatableTypes => [
        typeof(SettingsBase),
        typeof(SettingsFolder)
    ];

    public override string CustomNavigationWidget => "epi-cms/component/ContentNavigationTree";

    public override string CustomSelectTitle => LocalizationService.Current.GetString("/contentrepositories/globalsettings/customselecttitle");

    public override string Key => RepositoryKey;

    public override IEnumerable<Type> MainNavigationTypes =>
    [
        typeof(SettingsBase),
        typeof(SettingsFolder)
    ];

    public override IEnumerable<string> MainViews => [HomeView.ViewName];

    public override string Name => LocalizationService.Current.GetString("/contentrepositories/globalsettings/name");

    public override IEnumerable<ContentReference> Roots => new List<ContentReference>() { Settings.Service?.GlobalSettingsRoot ?? ContentReference.EmptyReference };

    public override int SortOrder => 1000;

    private Injected<ISettingsService> Settings { get; set; }
}
