using EPiServer.Cms.Shell.Search;
using EPiServer.Shell;
using EPiServer.Shell.Search;
using EPiServer.Web.Routing;

namespace lunchin.Optimizely.Cloud.Extensions.Taxonomy;

[SearchProvider]
public class ClassifiicationSearchProvider(
    LocalizationService localizationService,
    ISiteDefinitionResolver siteDefinitionResolver,
    IContentTypeRepository<ContentType> contentTypeRepository,
    EditUrlResolver editUrlResolver,
    ServiceAccessor<SiteDefinition> currentSiteDefinition,
    IContentLanguageAccessor languageResolver,
    UrlResolver urlResolver,
    TemplateResolver templateResolver,
    UIDescriptorRegistry uiDescriptorRegistry,
    IContentLoader contentLoader) : ContentSearchProviderBase<ClassificationData, ContentType>(
        localizationService: localizationService,
        siteDefinitionResolver: siteDefinitionResolver,
        contentTypeRepository: contentTypeRepository,
        editUrlResolver: editUrlResolver,
        currentSiteDefinition: currentSiteDefinition,
        languageResolver: languageResolver,
        urlResolver: urlResolver,
        templateResolver: templateResolver,
        uiDescriptorRegistry: uiDescriptorRegistry)
{
    internal const string SearchArea = "Taxonomy/classfications";
    private readonly IContentLoader _contentLoader = contentLoader;
    private readonly LocalizationService _localizationService = localizationService;

    public override string Area => SearchArea;

    public override string Category => _localizationService.GetString("/episerver/cms/components/taxonomy/title");

    protected override string IconCssClass => "epi-iconSettings";

    public override IEnumerable<SearchResult> Search(Query query)
    {
        if (string.IsNullOrWhiteSpace(value: query?.SearchQuery) || query.SearchQuery.Trim().Length < 2)
        {
            return [];
        }

        var searchResultList = new List<SearchResult>();
        var str = query.SearchQuery.Trim();

        var descendants = _contentLoader.GetItems(_contentLoader.GetDescendents(ClassificationFolder.ClassificationRoot), LanguageLoaderOption.FallbackWithMaster().Language)
            .OfType<ClassificationData>();

        foreach (var setting in descendants)
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
                   : $"{((ClassificationData)content).Name} {_localizationService.GetString("/contentrepositories/taxonomy/customselecttitle").ToLower()}";
    }
}
