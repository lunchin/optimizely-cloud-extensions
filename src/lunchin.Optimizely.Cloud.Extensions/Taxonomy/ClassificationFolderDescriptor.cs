using EPiServer.Shell;

namespace lunchin.Optimizely.Cloud.Extensions.Taxonomy;

[UIDescriptorRegistration]
public class ClassificationFolderDescriptor : UIDescriptor<ClassificationFolder>
{
    public ClassificationFolderDescriptor()
    {
        IsPrimaryType = true;
    }
}
