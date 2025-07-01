using EPiServer.Shell.ObjectEditing;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions.UI;

public class HideCategoryPropertyExtender(IOptions<ExtensionsOptions> options) : IMetadataExtender
{
    private readonly ExtensionsOptions _options = options.Value;

    public void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
    {
        if (_options?.HideDefaultCategoryEnabled ?? false)
        {
            if (metadata.Model is IContent data)
            {
                try
                {
                    var category = metadata?.Properties?.FirstOrDefault(x => x.PropertyName == "icategorizable_category");
                    if (category != null)
                    {
                        category.ShowForEdit = false;
                    }
                }
                catch { }
            }
        }
    }
}
