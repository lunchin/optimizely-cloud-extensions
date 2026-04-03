using EPiServer.Data;
using EPiServer.Web.Routing;
using lunchin.Optimizely.Cloud.Extensions.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using sample.Models;
using sample.Models.Pages;
using sample.Models.ViewModels;

namespace sample.Business;

[ServiceConfiguration]
public class PageViewContextFactory(
    IContentLoader contentLoader,
    UrlResolver urlResolver,
    IDatabaseMode databaseMode,
    IOptionsMonitor<CookieAuthenticationOptions> optionMonitor,
    ISettingsService settingsService)
{
    private readonly IContentLoader _contentLoader = contentLoader;
    private readonly UrlResolver _urlResolver = urlResolver;
    private readonly IDatabaseMode _databaseMode = databaseMode;
    private readonly CookieAuthenticationOptions _cookieAuthenticationOptions = optionMonitor.Get(IdentityConstants.ApplicationScheme);
    private readonly ISettingsService _settingsService = settingsService;

    public virtual LayoutModel CreateLayoutModel(ContentReference currentContentLink, HttpContext httpContext)
    {
        var settings = _settingsService.GetSiteSettings<SiteSettings>();

        return new LayoutModel
        {
            Logotype = settings.SiteLogotype,
            LogotypeLinkUrl = new HtmlString(_urlResolver.GetUrl(ContentReference.StartPage)),
            ProductPages = settings.ProductPageLinks,
            CompanyInformationPages = settings.CompanyInformationPageLinks,
            NewsPages = settings.NewsPageLinks,
            CustomerZonePages = settings.CustomerZonePageLinks,
            LoggedIn = httpContext.User.Identity.IsAuthenticated,
            LoginUrl = new HtmlString(GetLoginUrl(currentContentLink)),
            SearchActionUrl = new HtmlString(UrlResolver.Current.GetUrl(settings.SearchPageLink)),
            IsInReadonlyMode = _databaseMode.DatabaseMode == DatabaseMode.ReadOnly
        };
    }

    private string GetLoginUrl(ContentReference returnToContentLink)
    {
        return $"{_cookieAuthenticationOptions?.LoginPath.Value ?? Globals.LoginPath}?ReturnUrl={_urlResolver.GetUrl(returnToContentLink)}";
    }

    public virtual IContent GetSection(ContentReference contentLink)
    {
        var currentContent = _contentLoader.Get<IContent>(contentLink);

        static bool isSectionRoot(ContentReference contentReference) =>
           ContentReference.IsNullOrEmpty(contentReference) ||
           contentReference.Equals(ContentReference.StartPage) ||
           contentReference.Equals(ContentReference.RootPage);

        return isSectionRoot(currentContent.ParentLink)
            ? currentContent
            : _contentLoader.GetAncestors(contentLink)
            .OfType<PageData>()
            .SkipWhile(x => !isSectionRoot(x.ParentLink))
            .FirstOrDefault();
    }
}
