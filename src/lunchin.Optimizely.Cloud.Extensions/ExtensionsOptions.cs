namespace lunchin.Optimizely.Cloud.Extensions;

public class ExtensionsOptions
{
    public const string Path = "loce:extensions";

    public List<Site> Sites { get; set; } = [];

    public bool EnvironmentBannerEnabled { get; set; } = true;

    public string EnvironmentBannerBackgroundColor { get; set; } = "#2cd31f";

    public string EnvironmentBannerTextColor { get; set; } = "#ffffff";

    public bool StorageExplorerEnabled { get; set; } = true;

    public bool MasterLanguageEnabled { get; set; } = true;

    public bool SettingsEnabled { get; set; } = true;

    public bool SiteHostnameInitializationEnabled { get; set; } = true;

    public bool ImagePreviewEnabled { get; set; } = true;

    public string AltTextPropertyName { get; set; } = "AltText";

    public bool TaxonomyEnabled { get; set; } = true;

    public bool HideDefaultCategoryEnabled { get; set; } = true;
}

public class Site
{
    public string? Name { get; set; }

    public string? Hostname { get; set; }
}
