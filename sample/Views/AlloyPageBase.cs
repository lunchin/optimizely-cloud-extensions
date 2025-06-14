using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Razor.TagHelpers;
using sample.Business.Rendering;

namespace sample.Views;

public abstract class AlloyPageBase<TModel> : RazorPage<TModel> where TModel : class
{
    private readonly AlloyContentAreaItemRenderer _alloyContentAreaItemRenderer;

    public abstract override Task ExecuteAsync();

    protected AlloyPageBase() : this(ServiceLocator.Current.GetInstance<AlloyContentAreaItemRenderer>())
    {
    }

    protected AlloyPageBase(AlloyContentAreaItemRenderer alloyContentAreaItemRenderer)
    {
        _alloyContentAreaItemRenderer = alloyContentAreaItemRenderer;
    }

    protected void OnItemRendered(ContentAreaItem contentAreaItem, TagHelperContext context, TagHelperOutput output)
    {
        _alloyContentAreaItemRenderer.RenderContentAreaItemCss(contentAreaItem, context, output);
    }
}
