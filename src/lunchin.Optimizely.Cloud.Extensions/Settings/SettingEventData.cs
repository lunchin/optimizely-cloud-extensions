using System.Runtime.Serialization;
using EPiServer.Events;

namespace lunchin.Optimizely.Cloud.Extensions.Settings;

[DataContract]
[EventsServiceKnownType]
public class SettingEventData
{
    [DataMember]
    public string? SiteId { get; set; }

    [DataMember]
    public string? ContentId { get; set; }

    [DataMember]
    public string? Language { get; set; }
}
