using EPiServer.Web.Routing;

namespace sample.Helpers;

public static class UrlHelpers
{
    /// <summary>
    /// Returns the target URL for a ContentReference. Respects the page's shortcut setting
    /// so if the page is set as a shortcut to another page or an external URL that URL
    /// will be returned.
    /// </summary>
    public static string PageLinkUrl(this IUrlHelper urlHelper, ContentReference contentLink)
    {
        if (ContentReference.IsNullOrEmpty(contentLink))
        {
            return string.Empty;
        }

        var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
        var page = contentLoader.Get<PageData>(contentLink);

        return PageLinkUrl(urlHelper, page);
    }

    /// <summary>
    /// Returns the target URL for a page. Respects the page's shortcut setting
    /// so if the page is set as a shortcut to another page or an external URL that URL
    /// will be returned.
    /// </summary>
    public static string PageLinkUrl(this IUrlHelper urlHelper, PageData page)
    {
        var urlResolver = urlHelper.ActionContext.HttpContext.RequestServices.GetRequiredService<UrlResolver>();
        switch (page.LinkType)
        {
            case PageShortcutType.Normal:
            case PageShortcutType.FetchData:
                return urlResolver.GetUrl(page.ContentLink);

            case PageShortcutType.Shortcut:
                if (page.Property["PageShortcutLink"] is PropertyPageReference shortcutProperty &&
                    !ContentReference.IsNullOrEmpty(shortcutProperty.ContentLink))
                {
                    return urlHelper.PageLinkUrl(shortcutProperty.ContentLink);
                }
                break;

            case PageShortcutType.External:
                return page.LinkURL;
            case PageShortcutType.Inactive:
                break;
            default:
                break;
        }
        return string.Empty;
    }

    /// <summary>
    /// Returns the URL anchor target for a page with shortcut settings
    /// </summary>
    public static string PageLinkTarget(this IUrlHelper urlHelper, PageData page)
    {
        return page.LinkType switch
        {
            PageShortcutType.Normal => "",
            PageShortcutType.Inactive => "",
            PageShortcutType.FetchData => page.TargetFrameName,
            PageShortcutType.Shortcut => page.TargetFrameName,
            PageShortcutType.External => page.TargetFrameName,
            _ => throw new ArgumentOutOfRangeException($"Unknown link type: {page.LinkType}'")
        };
    }
}
