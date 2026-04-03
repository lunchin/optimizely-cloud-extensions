using System.Collections.Concurrent;
using EPiServer.Applications;
using EPiServer.Events;

namespace lunchin.Optimizely.Cloud.Extensions.Settings;

public interface ISettingsService : IEventSubscriber<ApplicationCreatedEvent>,
        IEventSubscriber<ApplicationUpdatedEvent>,
        IEventSubscriber<ApplicationDeletedEvent>,
        IEventSubscriber<SettingEventData>
{
    ContentReference? GlobalSettingsRoot { get; set; }
    ConcurrentDictionary<string, Dictionary<Type, Guid>> SiteSettings { get; }
    T? GetSiteSettings<T>(string? siteName = null, string? language = null) where T : SettingsBase;
    void InitializeSettings();
    void RegisterContentRoots();
    void UpdateSettings(string siteName, IContent content);
    void UpdateSettings();
}
