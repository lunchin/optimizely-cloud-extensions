using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using EPiServer.Web.Routing;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions.UI;

[EditorDescriptorRegistration(TargetType = typeof(string), UIHint = "FormImagePreview", EditorDescriptorBehavior = EditorDescriptorBehavior.OverrideDefault)]
public class ImagePreviewEditorDescriptor(IOptions<ExtensionsOptions> options) : EditorDescriptor
{
    private readonly ExtensionsOptions _options = options.Value;

    public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
    {
        if (_options?.ImagePreviewEnabled ?? false)
        {
            ClientEditingClass = "loce/formImagePreviewWidget";
            if (metadata?.Parent?.Model is ImageData image)
            {
                metadata.EditorConfiguration["src"] = ServiceLocator.Current.GetInstance<IUrlResolver>().GetUrl(image);
            }
        }
        base.ModifyMetadata(metadata, attributes);
    }
}
