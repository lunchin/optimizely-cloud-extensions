namespace lunchin.Optimizely.Cloud.Extensions;

public class ExtensionsOptions
{
    public const string Path = "loce:extensions";

    public List<Site> Sites { get; set; } = [];

    public bool EnvironmentBannerEnabled { get; set; } = true;

    public string EnvironmentBannerBaackgroundColor { get; set; } = "#2cd31f";

    public string EnvironmentBannerTextColor { get; set; } = "#ffffff";

    public bool StorageExplorerEnabled { get; set; } = true;

    public bool MasterLanguageEnabled { get; set; } = true;

    public bool SettingsEnabled { get; set; } = true;

    public bool SiteHostnameInitilizationEnabled { get; set; } = true;

    public string AltTextPropertyName { get; set; } = "AltText";
}

public class Site
{
    public string? Name { get; set; }

    public string? Hostname { get; set; }
}
