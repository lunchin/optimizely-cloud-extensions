using EPiServer.Framework.Localization;
using EPiServer.Shell;
using EPiServer.Shell.Navigation;
using EPiServer.Shell.Navigation.Internal;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions.Commerce;

[MenuProvider]
public class MenuProvider(LocalizationService localizationService, IOptions<CommerceExtensionsOptions> options) : IMenuProvider, IEPiProductMenuProvider
{
    private const string LunchinMenuPath = MenuPaths.Global + "/lunchin";
    private readonly LocalizationService _localizationService = localizationService;
    private readonly CommerceExtensionsOptions _options = options.Value;

    public IEnumerable<MenuItem> GetMenuItems()
    {
        if (_options?.MultiCouponsEnabled != true)
        {
            return [];
        }

        var url = Paths.ToResource(GetType(), "multicoupons");
        return
        [
            new SectionMenuItem(_localizationService.GetString("lunchin/menutitle", "lunchin Extensions"), LunchinMenuPath)
            {
                SortIndex = SortIndex.Last + 1,
                AuthorizationPolicy = Constants.lunchinPolicy
            },
            new UrlMenuItem(_localizationService.GetString("lunchin/menuitem", "Multi Coupon Discounts"), $"{LunchinMenuPath}/multicoupon", url)
            {
                SortIndex = SortIndex.Last + 10,
                AuthorizationPolicy = Constants.lunchinPolicy,
                IconName = "tag"
            }
        ];
    }
}
