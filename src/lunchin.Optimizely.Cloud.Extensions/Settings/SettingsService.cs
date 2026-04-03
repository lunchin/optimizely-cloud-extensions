using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using EPiServer.Applications;
using EPiServer.DataAccess;
using EPiServer.Events;
using EPiServer.Framework.TypeScanner;
using EPiServer.Logging;
using EPiServer.Security;

namespace lunchin.Optimizely.Cloud.Extensions.Settings;

public class SettingsService(
    IContentRepository contentRepository,
    ContentRootService contentRootService,
    ITypeScannerLookup typeScannerLookup,
    IContentTypeRepository contentTypeRepository,
    IContentEvents contentEvents,
    IApplicationResolver applicationResolver,
    IApplicationRepository applicationRepository,
    IHttpContextAccessor httpContext,
    IEventPublisher eventPublisher,
    IContentLanguageAccessor contentLanguageAccessor) :
        ISettingsService,
        IDisposable
        
{
    private readonly IContentRepository _contentRepository = contentRepository;
    private readonly ContentRootService _contentRootService = contentRootService;
    private readonly IContentTypeRepository _contentTypeRepository = contentTypeRepository;
    private readonly ILogger _log = LogManager.GetLogger();
    private readonly ITypeScannerLookup _typeScannerLookup = typeScannerLookup;
    private readonly IContentEvents _contentEvents = contentEvents;
    private readonly IApplicationRepository _applicationRepository = applicationRepository;
    private readonly IApplicationResolver _applicationResolver = applicationResolver;
    private readonly IHttpContextAccessor _httpContext = httpContext;
    private readonly IEventPublisher _eventPublisher = eventPublisher;
    private readonly IContentLanguageAccessor _contentLanguageAccessor = contentLanguageAccessor;

    public ConcurrentDictionary<string, Dictionary<Type, Guid>> SiteSettings { get; } = new ConcurrentDictionary<string, Dictionary<Type, Guid>>();

    public ContentReference? GlobalSettingsRoot { get; set; }

    public List<T> GetAllSiteSettings<T>() where T : SettingsBase
    {
        var sites = _applicationRepository.List();
        var siteSettings = new List<T>();

        foreach (var site in sites)
        {
            var settings = GetSiteSettings<T>(site.Name);
            if (settings != null)
            {
                siteSettings.Add(settings);
            }
        }

        return siteSettings;
    }

    public T? GetSiteSettings<T>(string? name = null, string? language = null) where T : SettingsBase
    {
        if (string.IsNullOrEmpty(name))
        {
            name = ResolveSiteName();
            if (string.IsNullOrEmpty(name))
            {
                return default;
            }
        }

        try
        {
            if (SiteSettings.TryGetValue(name, out var siteSettings) &&
                siteSettings.TryGetValue(typeof(T), out var settingId))
            {
                return _contentRepository.Get<T>(settingId, language == null ? _contentLanguageAccessor.Language : CultureInfo.GetCultureInfo(language));
            }
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            _log.Error($"[Settings] {keyNotFoundException.Message}", exception: keyNotFoundException);
        }
        catch (ArgumentNullException argumentNullException)
        {
            _log.Error($"[Settings] {argumentNullException.Message}", exception: argumentNullException);
        }

        return default;
    }

    public void InitializeSettings()
    {
        try
        {
            RegisterContentRoots();
        }
        catch (NotSupportedException notSupportedException)
        {
            _log.Error($"[Settings] {notSupportedException.Message}", exception: notSupportedException);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _log.Error(ex.Message, ex);
        }
        _contentEvents.PublishedContent += PublishedContent;
    }

    public void UpdateSettings(string name, IContent content)
    {
        var contentType = content.GetOriginalType();
        try
        {
            if (!SiteSettings.TryGetValue(name, out var value))
            {
                value = [];
                SiteSettings[name] = value;
            }

            value[contentType] = content.ContentGuid;
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            _log.Error($"[Settings] {keyNotFoundException.Message}", exception: keyNotFoundException);
        }
        catch (ArgumentNullException argumentNullException)
        {
            _log.Error($"[Settings] {argumentNullException.Message}", exception: argumentNullException);
        }
    }

    public void UpdateSettings()
    {
        var root = _contentRepository.GetItems(_contentRootService.List(), [])
             .FirstOrDefault(x => x.ContentGuid == SettingsFolder.SettingsRootGuid);

        if (root == null)
        {
            return;
        }

        GlobalSettingsRoot = root.ContentLink;
        var children = _contentRepository.GetChildren<SettingsFolder>(GlobalSettingsRoot).ToList();
        foreach (var site in _applicationRepository.List())
        {
            var folder = children.Find(x => x.Name.Equals(site.Name, StringComparison.InvariantCultureIgnoreCase));
            if (folder == null)
            {
                CreateSiteFolder(site);
                return;
            }

            var settingsModelTypes = _typeScannerLookup.AllTypes
                    .Where(t => t.GetCustomAttributes(typeof(SettingsContentTypeAttribute), false).Length > 0);

            foreach (var settingsType in settingsModelTypes)
            {
                if (settingsType.GetCustomAttributes(typeof(SettingsContentTypeAttribute), false)
                    .FirstOrDefault() is not SettingsContentTypeAttribute attribute)
                {
                    continue;
                }

                var siteSetting = _contentRepository.GetChildren<SettingsBase>(folder.ContentLink, _contentLanguageAccessor.Language)
                    .FirstOrDefault(x => x.Name.Equals(attribute.SettingsName));

                if (siteSetting != null)
                {
                    UpdateSettings(site.Name, siteSetting);
                }
                else
                {
                    var contentType = _contentTypeRepository.Load(settingsType);
                    var newSettings = _contentRepository.GetDefault<IContent>(folder.ContentLink, contentType.ID);
                    newSettings.Name = attribute.SettingsName;

                    try
                    {
                        _ = _contentRepository.Save(newSettings, SaveAction.Publish, AccessLevel.NoAccess);
                        UpdateSettings(site.Name, newSettings);
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message);
                    }
                }

            }
        }
    }

    public void RegisterContentRoots()
    {
        var registeredRoots = _contentRepository.GetItems(_contentRootService.List(), []);
        var settingsRootRegistered = registeredRoots.Any(x => x.ContentGuid == SettingsFolder.SettingsRootGuid && x.Name.Equals(SettingsFolder.SettingsRootName));

        if (!settingsRootRegistered)
        {
            _contentRootService.Register<SettingsFolder>(SettingsFolder.SettingsRootName, SettingsFolder.SettingsRootGuid, ContentReference.RootPage);
        }

        UpdateSettings();
    }

    public void Dispose()
    {
        _contentEvents?.PublishedContent -= PublishedContent;
        GC.SuppressFinalize(this);
    }

    private void CreateSiteFolder(Application application)
    {
        var folder = _contentRepository.GetDefault<SettingsFolder>(GlobalSettingsRoot);
        folder.Name = application.Name;
        var reference = _contentRepository.Save(folder, SaveAction.Publish, AccessLevel.NoAccess);

        var settingsModelTypes = _typeScannerLookup.AllTypes
            .Where(t => t.GetCustomAttributes(typeof(SettingsContentTypeAttribute), false).Length > 0);

        foreach (var settingsType in settingsModelTypes)
        {
            if (settingsType.GetCustomAttributes(typeof(SettingsContentTypeAttribute), false)
                .FirstOrDefault() is not SettingsContentTypeAttribute attribute)
            {
                continue;
            }

            var contentType = _contentTypeRepository.Load(settingsType);
            var newSettings = _contentRepository.GetDefault<IContent>(reference, contentType.ID);
            newSettings.Name = attribute.SettingsName;

            try
            {
                _contentRepository.Save(newSettings, SaveAction.Publish, AccessLevel.NoAccess);
                UpdateSettings(application.Name, newSettings);
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }
    }

    public Task HandleAsync(ApplicationCreatedEvent eventData, EventContext context, CancellationToken cancellationToken = default)
    {
        if (_contentRepository.GetChildren<SettingsFolder>(GlobalSettingsRoot)
            .Any(x => x.Name.Equals(eventData.Application.Name, StringComparison.InvariantCultureIgnoreCase)))
        {
            return Task.CompletedTask;
        }

        CreateSiteFolder(eventData.Application);
        return Task.CompletedTask;
    }

    public Task HandleAsync(ApplicationDeletedEvent eventData, EventContext context, CancellationToken cancellationToken = default)
    {
        var folder = _contentRepository.GetChildren<SettingsFolder>(GlobalSettingsRoot)
            .FirstOrDefault(x => x.Name.Equals(eventData.Application.Name, StringComparison.InvariantCultureIgnoreCase));

        if (folder == null)
        {
            return Task.CompletedTask;
        }

        _contentRepository.Delete(folder.ContentLink, true, AccessLevel.NoAccess);
        return Task.CompletedTask;
    }

    public Task HandleAsync(ApplicationUpdatedEvent eventData, EventContext context, CancellationToken cancellationToken = default)
    {

        var prevSite = eventData.PreviousApplication.Name;
        var updatedSite = eventData.Application.Name;
        var settingsRoot = GlobalSettingsRoot;
        if (_contentRepository.GetChildren<IContent>(settingsRoot)
            .FirstOrDefault(x => x.Name.Equals(prevSite, StringComparison.InvariantCultureIgnoreCase)) is ContentFolder currentSettingsFolder)
        {
            var cloneFolder = currentSettingsFolder.CreateWritableClone();
            cloneFolder.Name = updatedSite;
            _contentRepository.Save(cloneFolder);
            return Task.CompletedTask;
        }


        CreateSiteFolder(eventData.Application);
        return Task.CompletedTask;
    }

    public Task HandleAsync(SettingEventData eventData, EventContext context, CancellationToken cancellationToken = default)
    {
        // don't process events locally raised
        if (context.Broadcasted && eventData != null)
        {
            if (Guid.TryParse(eventData.ContentId, out var contentId))
            {
                var content = _contentRepository.Get<IContent>(contentId);
                if (content != null && eventData.SiteId != null)
                {
                    UpdateSettings(eventData.SiteId, content);
                }
            }
        }
        return Task.CompletedTask;
    }

    private void PublishedContent(object? sender, ContentEventArgs e)
    {
        if (e?.Content is not SettingsBase)
        {
            return;
        }

        var parent = _contentRepository.Get<IContent>(e.Content.ParentLink);
        var site = _applicationRepository.Get(parent.Name);

        var id = site?.Name;
        if (string.IsNullOrEmpty(id))
        {
            return;
        }
        UpdateSettings(id, e.Content);
        _eventPublisher.PublishAsync(new SettingEventData
        {
            SiteId = id,
            ContentId = e.Content.ContentGuid.ToString()
        }).GetAwaiter().GetResult();
    }

    private string? ResolveSiteName()
    {
        var request = _httpContext.HttpContext?.Request;
        if (request == null)
        {
            return null;
        }

        var site = _applicationResolver.GetByHostname(request.Host.Value ?? "", false);
        if (site?.Application != null)
        {
            return site.Application.Name;
        }

        return null;
    }
}
