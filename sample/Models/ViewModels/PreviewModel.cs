using sample.Models.Pages;

namespace sample.Models.ViewModels;

public class PreviewModel : PageViewModel<SitePageData>
{
    public PreviewModel(SitePageData currentPage, IContent previewContent)
        : base(currentPage)
    {
        PreviewContent = previewContent;
        Areas = [];
    }

    public IContent PreviewContent { get; set; }

    public List<PreviewArea> Areas { get; set; }

    public class PreviewArea
    {
        public bool Supported { get; set; }

        public string AreaName { get; set; }

        public string AreaTag { get; set; }

        public ContentArea ContentArea { get; set; }
    }
}
