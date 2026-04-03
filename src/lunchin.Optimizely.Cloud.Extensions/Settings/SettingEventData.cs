using System.Runtime.Serialization;
using EPiServer.Events;

namespace lunchin.Optimizely.Cloud.Extensions.Settings;

[DataContract]
[EventData("b29b8aef-2a17-4432-ad16-8cd6cc6953e3", Broadcast = true)]
public class SettingEventData : IEventData
{
    [DataMember]
    public string? SiteId { get; set; }

    [DataMember]
    public string? ContentId { get; set; }

    [DataMember]
    public string? Language { get; set; }
}
