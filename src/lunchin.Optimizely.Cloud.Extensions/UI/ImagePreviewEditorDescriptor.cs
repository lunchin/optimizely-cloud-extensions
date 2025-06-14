using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using EPiServer.Web.Routing;

namespace lunchin.Optimizely.Cloud.Extensions.UI;

[EditorDescriptorRegistration(TargetType = typeof(string), UIHint = "FormImagePreview", EditorDescriptorBehavior = EditorDescriptorBehavior.OverrideDefault)]
public class ImagePreviewEditorDescriptor : EditorDescriptor
{
    public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
    {
        ClientEditingClass = "loce/widgets/formImagePreviewWidget";

        if (metadata.Parent.Model is ImageData image)
        {
            metadata.EditorConfiguration["src"] = ServiceLocator.Current.GetInstance<IUrlResolver>().GetUrl(image);
        }

        base.ModifyMetadata(metadata, attributes);
    }
}
