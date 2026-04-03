using EPiServer.Applications;
using EPiServer.Cms.Shell.Search;
using EPiServer.Shell;
using EPiServer.Shell.Search;
using EPiServer.Web.Routing;

namespace lunchin.Optimizely.Cloud.Extensions.Settings;

[SearchProvider]
public class GlobalSettingsSearchProvider(
    LocalizationService localizationService,
    IApplicationResolver applicationResolver,
    IContentTypeRepository contentTypeRepository,
    EditUrlResolver editUrlResolver,
    IContentLanguageAccessor languageResolver,
    UrlResolver urlResolver,
    UIDescriptorRegistry uiDescriptorRegistry,
    IContentLoader contentLoader,
    ISettingsService settingsService) : ContentSearchProviderBase<SettingsBase, ContentType>(
        localizationService: localizationService,
        applicationResolver: applicationResolver,
        contentTypeRepository: contentTypeRepository,
        editUrlResolver: editUrlResolver,
        languageResolver: languageResolver,
        urlResolver: urlResolver,
        uiDescriptorRegistry: uiDescriptorRegistry)
{
    internal const string _searchArea = "Settings/globalsettings";
    private readonly IContentLoader _contentLoader = contentLoader;
    private readonly LocalizationService _localizationService = localizationService;
    private readonly ISettingsService _settingsService = settingsService;

    public override string Area => _searchArea;

    public override string Category => _localizationService.GetString("/episerver/cms/components/globalsettings/title");

    protected override string IconCssClass => "epi-iconSettings";

    public override IEnumerable<SearchResult> Search(Query query)
    {
        if (string.IsNullOrWhiteSpace(value: query?.SearchQuery) || query.SearchQuery.Trim().Length < 2)
        {
            return [];
        }

        var searchResultList = new List<SearchResult>();
        var str = query.SearchQuery.Trim();

        var globalSettings =
            _contentLoader.GetChildren<SettingsBase>(contentLink: _settingsService.GlobalSettingsRoot);

        foreach (var setting in globalSettings)
        {
            if (setting.Name.IndexOf(value: str, comparisonType: StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            searchResultList.Add(CreateSearchResult(contentData: setting));

            if (searchResultList.Count == query.MaxResults)
            {
                break;
            }
        }

        return searchResultList;
    }

    protected override string CreatePreviewText(IContentData content)
    {
        return content == null
                   ? string.Empty
                   : $"{((SettingsBase)content).Name} {_localizationService.GetString("/contentrepositories/globalsettings/customselecttitle").ToLower()}";
    }

    protected override string GetEditUrl(SettingsBase contentData, out bool onCurrentHost)
    {
        onCurrentHost = true;

        if (contentData == null)
        {
            return string.Empty;
        }

        var contentLink = contentData.ContentLink;
        var language = string.Empty;
        var localizable = contentData;

        if (localizable != null)
        {
            language = localizable.Language.Name;
        }

        return
            $"/episerver/lunchin.Optimizely.Cloud.Extensions/settings#context=epi.cms.contentdata:///{contentLink.ID}&viewsetting=viewlanguage:///{language}";
    }
}
