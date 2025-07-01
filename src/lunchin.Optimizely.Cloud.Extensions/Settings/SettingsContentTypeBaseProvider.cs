using EPiServer.DataAbstraction.RuntimeModel;

namespace lunchin.Optimizely.Cloud.Extensions.Settings;

public class SettingsContentTypeBaseProvider : IContentTypeBaseProvider
{
    public IEnumerable<ContentTypeBase> ContentTypeBases => [new ContentTypeBase("Setting")];

    public Type? Resolve(ContentTypeBase contentTypeBase) => contentTypeBase.ToString().Equals("Setting") ? typeof(SettingsBase) : null;
}
