namespace lunchin.Optimizely.Cloud.Extensions.Settings;

[ContentType(GUID = "38e8d262-ad43-4fc9-8b1f-3fc03e41ea26")]
[AvailableContentTypes(Include = [typeof(SettingsBase), typeof(SettingsFolder)])]
public class SettingsFolder : ContentFolder
{
    public const string SettingsRootName = "SettingsRoot";
    public static Guid SettingsRootGuid = new("9594b873-5e96-4fac-ac55-87303fc88a41");

    private Injected<LocalizationService> _localizationService;
    private static Injected<ContentRootService> _rootService;

    public static ContentReference SettingsRoot => GetSettingsRoot();

    public override string Name
    {
        get
        {
            return ContentLink.CompareToIgnoreWorkID(SettingsRoot)
                ? _localizationService.Service.GetString("/contentrepositories/globalsettings/Name")
                : base.Name;
        }
        set => base.Name = value;
    }

    private static ContentReference GetSettingsRoot() => _rootService.Service.Get(SettingsRootName);
}
