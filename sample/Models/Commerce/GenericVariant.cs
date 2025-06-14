using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.DataAnnotations;

namespace sample.Models.Commerce;

[CatalogContentType(DisplayName = "Generic Variant",
    GUID = "1aaa2c58-c424-4c37-81b0-77e76d254eb0",
    Description = "Generic variant supports multiple variation types")]
public class GenericVariant : VariationContent
{
}
