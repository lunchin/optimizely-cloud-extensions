using System.Globalization;
using System.Text.Json;
using EPiServer.Core.Html.StringParsing;
using EPiServer.Globalization;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Routing;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions;

public static class HelperExtensions
{
    private static readonly Lazy<IContextModeResolver> _contextModeResolver = new(ServiceLocator.Current.GetInstance<IContextModeResolver>);
    private static readonly Lazy<IContentLoader> _contentLoader = new(ServiceLocator.Current.GetInstance<IContentLoader>);
    private static readonly Lazy<ExtensionsOptions> _extensionsOptions = new(() => ServiceLocator.Current.GetInstance<IOptions<ExtensionsOptions>>().Value);
    private static readonly Lazy<UrlResolver> _urlResolver = new(ServiceLocator.Current.GetInstance<UrlResolver>);
    private static readonly Lazy<IPermanentLinkMapper> _permanentLinkMapper = new(ServiceLocator.Current.GetInstance<IPermanentLinkMapper>);
    private static readonly Lazy<IUrlResolver> _iUrlResolver = new(ServiceLocator.Current.GetInstance<IUrlResolver>);
    private static readonly Lazy<ISiteDefinitionResolver> _siteDefinitionResolver = new(ServiceLocator.Current.GetInstance<ISiteDefinitionResolver>);
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static bool IsNullOrEmpty(this string input) => string.IsNullOrEmpty(input);

    public static Guid GetContentGuid(this ContentReference contentLink)
    {
        return ContentReference.IsNullOrEmpty(contentLink) ? Guid.Empty : _permanentLinkMapper.Value.Find(contentLink)?.Guid ?? Guid.Empty;
    }

    public static ContentReference GetContentReference(this Url contentUrl)
    {
        if (contentUrl == null || string.IsNullOrEmpty(contentUrl.ToString()))
        {
            return ContentReference.EmptyReference;
        }

        var guid = PermanentLinkUtility.GetGuid(contentUrl);
        if (guid == Guid.Empty)
        {
            return ContentReference.EmptyReference;
        }

        var content = GetContent(guid, shouldFallbackWithMaster: true);
        return content == null ? ContentReference.EmptyReference : content.ContentLink;
    }

    public static string GetUrl<T>(this T content, bool isAbsolute = false) where T : IContent, ILocale, IRoutable
    {
        return content.GetUri(isAbsolute).ToString();
    }

    public static Uri GetUri<T>(this T content, bool isAbsolute = false) where T : IContent, ILocale, IRoutable
    {
        return content.ContentLink.GetUri(content.Language.Name, isAbsolute);
    }

    public static Uri GetUri(this ContentReference contentRef, bool isAbsolute = false)
    {
        return contentRef.GetUri(ContentLanguage.PreferredCulture.Name, isAbsolute);
    }

    public static Uri GetUri(this ContentReference contentRef, string lang, bool isAbsolute = false)
    {
        var urlString = _iUrlResolver.Value.GetUrl(contentRef, lang, new UrlResolverArguments { ForceCanonical = true });
        if (string.IsNullOrEmpty(urlString))
        {
            return new Uri(string.Empty);
        }

        var uri = new Uri(urlString, UriKind.RelativeOrAbsolute);
        if (uri.IsAbsoluteUri || !isAbsolute)
        {
            return uri;
        }

        var siteDefinition = _siteDefinitionResolver.Value.GetByContent(contentRef, true, true);
        var host = siteDefinition.Hosts.FirstOrDefault(h => h.Type == HostDefinitionType.Primary) ?? siteDefinition.Hosts.FirstOrDefault(h => h.Type == HostDefinitionType.Undefined);
        var baseUrl = (host?.Name ?? "*").Equals("*") ? siteDefinition.SiteUrl : new Uri($"http{((host?.UseSecureConnection ?? false) ? "s" : string.Empty)}://{host?.Name ?? ""}");
        return new Uri(baseUrl, urlString);
    }

    public static IContent Get<TContent>(this ContentReference contentLink) where TContent : IContent => _contentLoader.Value.Get<TContent>(contentLink);

    public static IContent Get<TContent>(this ContentReference contentLink, string language) where TContent : IContent =>
        _contentLoader.Value.Get<TContent>(contentLink, CultureInfo.GetCultureInfo(language));

    public static string GetPublicUrl(this ContentReference contentLink, string? language = null)
    {
        if (ContentReference.IsNullOrEmpty(contentLink))
        {
            return string.Empty;
        }

        var content = GetContent(contentLink, shouldFallbackWithMaster: true, language);
        return content == null || !PublishedStateAssessor.IsPublished(content)
            ? string.Empty
            : _urlResolver.Value.GetUrl(contentLink, language);
    }

    public static string GetPublicUrl(this Guid contentGuid, string language)
    {
        return PermanentLinkUtility.FindContentReference(contentGuid).GetPublicUrl(language);
    }

    public static IList<T>? GetContentItems<T>(this IEnumerable<ContentAreaItem> contentAreaItems) where T : IContentData
    {
        return contentAreaItems == null || !contentAreaItems.Any()
            ? null
            : (IList<T>)_contentLoader.Value
            .GetItems(contentAreaItems.Select(x => x.ContentLink), [LanguageLoaderOption.FallbackWithMaster()])
            .OfType<T>()
            .ToList();
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var element in source)
        {
            action(element);
        }
    }

    public static string ToJson<T>(this T obj)
    {
        return JsonSerializer.Serialize(obj, _jsonSerializerOptions);
    }

    public static IHtmlContent AddAltTextToImages(this IHtmlHelper helper, XhtmlString html)
    {
        var content = helper.XhtmlString(html);
        if (content == null)
        {
            return new HtmlString(html.ToHtmlString());
        }

        if (_contextModeResolver.Value.CurrentMode == ContextMode.Edit)
        {
            return content;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(content.ToString());
        var imgs = doc.DocumentNode.SelectNodes("//img");
        if (imgs?.Any() != true)
        {
            return content;
        }

        var altTextDictionary = GenerateAltTextDictionary(html.Fragments);
        foreach (var img in imgs)
        {
            var src = img.Attributes["src"].Value;
            if (altTextDictionary.ContainsKey(src))
            {
                img.SetAttributeValue("alt", altTextDictionary[src]);
            }
        }

        return new HtmlString(doc.DocumentNode.OuterHtml);
    }

    private static Dictionary<string, string> GenerateAltTextDictionary(StringFragmentCollection htmlFragments)
    {
        var retVal = new Dictionary<string, string>();
        foreach (var urlFragment in htmlFragments.Where(x => x is UrlFragment))
        {
            foreach (var guid in urlFragment.ReferencedPermanentLinkIds)
            {
                if (!_contentLoader.Value.TryGet(guid, out ImageData image) && image.Property.Contains(_extensionsOptions.Value.AltTextPropertyName))
                {
                    continue;
                }

                var key = image.ContentLink.GetPublicUrl();
                if (!retVal.ContainsKey(key))
                {
                    retVal.Add(key, image.Property[_extensionsOptions.Value.AltTextPropertyName].Value?.ToString() ?? "Default image text");
                }
            }
        }
        return retVal;
    }

    private static IContent? GetContent(object contentIdentity, bool shouldFallbackWithMaster, string? language = null)
    {
        if (contentIdentity == null)
        {
            return null;
        }

        var loaderOptions = !TryGetCulture(language, out var culture)
            ? shouldFallbackWithMaster
                ? [LanguageLoaderOption.FallbackWithMaster()]
                : [LanguageLoaderOption.Fallback()]
            : (LoaderOptions)(shouldFallbackWithMaster
                ? [LanguageLoaderOption.FallbackWithMaster(culture)]
                : [LanguageLoaderOption.Fallback(culture)]);
        IContent? content;

        if (contentIdentity is Guid guid)
        {
            _contentLoader.Value.TryGet(guid, loaderOptions, out content);
        }
        else
        {
            if (contentIdentity is not ContentReference)
            {
                throw new NotSupportedException("Only support Guid or ContentReference as contentIdentity");
            }

            _contentLoader.Value.TryGet((ContentReference)contentIdentity, loaderOptions, out content);
        }

        return content;
    }

    private static bool TryGetCulture(string? languageCode, out CultureInfo? culture)
    {
        culture = null;
        if (languageCode == null)
        {
            return false;
        }

        try
        {
            culture = CultureInfo.GetCultureInfo(languageCode);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
