using EPiServer.Shell;
using EPiServer.Shell.Navigation;
using EPiServer.Shell.Navigation.Internal;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions;

[MenuProvider]
public class MenuProvider : IMenuProvider, IEPiProductMenuProvider
{
    private const string LunchinMenuPath = MenuPaths.Global + "/lunchin";
    private readonly LocalizationService _localizationService;
    private readonly ExtensionsOptions _options;

    public MenuProvider(LocalizationService localizationService, IOptions<ExtensionsOptions> options)
    {
        _localizationService = localizationService;
        _options = options.Value;
    }

    public IEnumerable<MenuItem> GetMenuItems()
    {
        if (_options?.StorageExplorerEnabled != true && _options?.MasterLanguageEnabled != true)
        {
            return [];
        }

        var storageurl = Paths.ToResource(GetType(), "storageexplorer");
        var masterLanguageUrl = Paths.ToResource(GetType(), "masterlanguageswitcher");
        var items = new List<MenuItem>
        {
            new SectionMenuItem(_localizationService.GetString("lunchin/menutitle", "lunchin Extensions"), LunchinMenuPath)
            {
                SortIndex = SortIndex.Last + 1,
                AuthorizationPolicy = Constants.lunchinPolicy,
            }
        };

        if (_options?.StorageExplorerEnabled == true)
        {
            items.Add(new UrlMenuItem(_localizationService.GetString("storageexplorer/menuitem", "Storage Explorer"), $"{LunchinMenuPath}/storagexplorer", storageurl)
            {
                SortIndex = 1,
                AuthorizationPolicy = Constants.lunchinPolicy,
                IconName = "folder",
                ToolTip = _localizationService.GetString("storageexplorer/menuitem", "Storage Explorer")
            });
        }

        if (_options?.MasterLanguageEnabled == true)
        {
            items.Add(new UrlMenuItem(_localizationService.GetString("masterlanguage/menuitem", "Master Language Switcher"), $"{LunchinMenuPath}/masterlanguage", masterLanguageUrl)
            {
                SortIndex = 2,
                AuthorizationPolicy = Constants.lunchinPolicy,
                IconName = "language",
                ToolTip = _localizationService.GetString("masterlanguage/menuitem", "Master Language Switcher")
            });
        }
        return items;
    }
}
