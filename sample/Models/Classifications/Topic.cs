using lunchin.Optimizely.Cloud.Extensions.Taxonomy;
using static sample.Globals;

namespace sample.Models.Classifications;

[AvailableContentTypes(Availability = Availability.Specific, Include = [typeof(ClassificationData)])]
[ContentType(GUID = "be6f1bd4-5bba-498c-95be-d790b068a677",
    DisplayName = "Topic taxonomy",
    GroupName = GroupNames.Content,
    Order = 300)]
public class Topic : ClassificationData
{
}
