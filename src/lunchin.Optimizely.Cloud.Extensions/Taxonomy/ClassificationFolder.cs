namespace lunchin.Optimizely.Cloud.Extensions.Taxonomy;

[ContentType(GUID = "a6fcdb0f-6010-4e06-ae57-4420d64e20a3")]
[AvailableContentTypes(Include = [typeof(ClassificationData), typeof(ClassificationFolder)])]
public class ClassificationFolder : ContentFolder
{
    public const string ClassificationRootName = "ClassificationRoot";
    public static readonly Guid ClassificationRootGuid = new("2d2c4f74-4fe5-4b44-9c45-c84e3647e29a");

    private Injected<LocalizationService> _localizationService;
    private static Injected<ContentRootService> _rootService;

    public static ContentReference ClassificationRoot => GetClassificationRoot();

    public override string Name
    {
        get
        {
            if (ContentLink.CompareToIgnoreWorkID(ClassificationRoot))
            {
                var localizedFolderName = _localizationService.Service.GetString("/contentrepositories/taxonomy/Name");

                return string.IsNullOrEmpty(localizedFolderName) ? "Classifications" : localizedFolderName;
            }

            return base.Name;
        }
        set => base.Name = value;
    }

    private static ContentReference GetClassificationRoot() => _rootService.Service.Get(ClassificationRootName);
}
