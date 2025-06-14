using EPiServer.SpecializedProperties;
using lunchin.Optimizely.Cloud.Extensions.Settings;
using sample.Models.Blocks;

namespace sample.Models;

[SettingsContentType(DisplayName = "Site Settings",
    GUID = "322f5b0b-581c-4050-90ee-d5c74f75b375",
    Description = "Core Site Settings",
    AvailableInEditMode = true,
    SettingsName = "Site Settings")]
public class SiteSettings : SettingsBase
{
    [Display(GroupName = SystemTabNames.Content, Order = 300)]
    public virtual LinkItemCollection ProductPageLinks { get; set; }

    [Display(GroupName = SystemTabNames.Content, Order = 350)]
    public virtual LinkItemCollection CompanyInformationPageLinks { get; set; }

    [Display(GroupName = SystemTabNames.Content, Order = 400)]
    public virtual LinkItemCollection NewsPageLinks { get; set; }

    [Display(GroupName = SystemTabNames.Content, Order = 450)]
    public virtual LinkItemCollection CustomerZonePageLinks { get; set; }

    [Display(GroupName = SystemTabNames.Content)]
    public virtual PageReference GlobalNewsPageLink { get; set; }

    [Display(GroupName = SystemTabNames.Content)]
    public virtual PageReference ContactsPageLink { get; set; }

    [Display(GroupName = SystemTabNames.Content)]
    public virtual PageReference SearchPageLink { get; set; }

    [Display(GroupName = SystemTabNames.Content)]
    public virtual SiteLogotypeBlock SiteLogotype { get; set; }
}
