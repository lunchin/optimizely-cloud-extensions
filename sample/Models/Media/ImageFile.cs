using EPiServer.Framework.DataAnnotations;
using lunchin.Optimizely.Cloud.Extensions;

namespace sample.Models.Media;

[ContentType(GUID = "0A89E464-56D4-449F-AEA8-2BF774AB8730")]
[MediaDescriptor(ExtensionString = "jpg,jpeg,jpe,ico,gif,bmp,png")]
public class ImageFile : ImageData
{
    public virtual string Copyright { get; set; }

    [UIHint(Constants.FormImagePreview)]
    [Display(
           GroupName = SystemTabNames.Content,
           Name = "Preview",
           Order = 999999999)]
    public virtual string FormPreview { get; set; }
}
