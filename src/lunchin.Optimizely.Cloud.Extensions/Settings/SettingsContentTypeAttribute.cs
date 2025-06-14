namespace lunchin.Optimizely.Cloud.Extensions.Settings;

[AttributeUsage(validOn: AttributeTargets.Class)]
public sealed class SettingsContentTypeAttribute : ContentTypeAttribute
{
    public string? SettingsName { get; set; }
}
