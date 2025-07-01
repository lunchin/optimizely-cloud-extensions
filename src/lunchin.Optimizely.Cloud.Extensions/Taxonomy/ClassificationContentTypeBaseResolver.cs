using EPiServer.DataAbstraction.RuntimeModel;

namespace lunchin.Optimizely.Cloud.Extensions.Taxonomy;

public class ClassificationContentTypeBaseResolver : IContentTypeBaseProvider
{
    public IEnumerable<ContentTypeBase> ContentTypeBases => [new ContentTypeBase("Classification")];

    public Type? Resolve(ContentTypeBase contentTypeBase) => contentTypeBase.ToString().Equals("Classification") ? typeof(ClassificationData) : null;
}
