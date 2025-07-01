using EPiServer.Web.Routing;

namespace lunchin.Optimizely.Cloud.Extensions.Taxonomy;

public abstract class ClassificationData : StandardContentBase, IRoutable
{
    private string? _routeSegment;
    private bool _isModified;

    [UIHint(UIHint.PreviewableText)]
    [CultureSpecific]
    public virtual string? RouteSegment
    {
        get { return _routeSegment; }
        set
        {
            ThrowIfReadOnly();
            _isModified = true;
            _routeSegment = value;
        }
    }

    [Display(Order = 20)]
    [UIHint(UIHint.Textarea)]
    [CultureSpecific]
    public virtual string? Description { get; set; }

    [Display(Order = 30)]
    [CultureSpecific]
    public virtual bool IsSelectable { get; set; }

    protected override bool IsModified
    {
        get
        {
            if (!base.IsModified)
            {
                return _isModified;
            }

            return true;
        }
    }

    public override void SetDefaultValues(ContentType contentType)
    {
        base.SetDefaultValues(contentType);
        IsSelectable = true;
    }
}
