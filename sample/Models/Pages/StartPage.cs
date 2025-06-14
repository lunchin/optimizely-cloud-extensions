using sample.Models.Blocks;
using EPiServer.SpecializedProperties;

namespace sample.Models.Pages;

/// <summary>
/// Used for the site's start page and also acts as a container for site settings
/// </summary>
[ContentType(
    GUID = "19671657-B684-4D95-A61F-8DD4FE60D559",
    GroupName = Globals.GroupNames.Specialized)]
[SiteImageUrl]
[AvailableContentTypes(
    Availability.Specific,
    Include = new[]
    {
        typeof(ContainerPage),
        typeof(ProductPage),
        typeof(StandardPage),
        typeof(ISearchPage),
        typeof(LandingPage),
        typeof(ContentFolder) }, // Pages we can create under the start page...
    ExcludeOn = new[]
    {
        typeof(ContainerPage),
        typeof(ProductPage),
        typeof(StandardPage),
        typeof(ISearchPage),
        typeof(LandingPage)
    })] // ...and underneath those we can't create additional start pages
public class StartPage : SitePageData
{
    [Display(
        GroupName = SystemTabNames.Content,
        Order = 320)]
    [CultureSpecific]
    public virtual ContentArea MainContentArea { get; set; }
}
