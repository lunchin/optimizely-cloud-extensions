using EPiServer.Applications;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions;

public class SiteHostnameInitilization(IApplicationRepository applicationRepository,
                        IOptions<ExtensionsOptions> extensionsOptions) : IBlockingFirstRequestInitializer
{
    private const string _preproduction = "Preproduction";
    private const string _integration = "Integration";
    private readonly IApplicationRepository _applicationRepository = applicationRepository;
    private readonly ExtensionsOptions _extensionsOptions = extensionsOptions.Value;
    private readonly string _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";

    public bool CanRunInParallel => false;

    public async Task InitializeAsync(HttpContext httpContext)
    {
        if (!_extensionsOptions.SiteHostnameInitializationEnabled)
        {
            return;
        }

        UpdateSiteHostnames(httpContext);
        await Task.CompletedTask;
    }

    private void UpdateSiteHostnames(HttpContext context)
    {
        if (!_environment.Equals(Environments.Development) && !_environment.Equals(_integration) && !_environment.Equals(_preproduction))
        {
            return;
        }

        var sites = _applicationRepository.List();
        if (!sites?.Any() ?? true)
        {
            return;
        }

        var request = context.Request;
        foreach (var siteHostname in _extensionsOptions.Sites)
        {
            if (sites?.FirstOrDefault(x => x.Name.Equals(siteHostname.Name, StringComparison.OrdinalIgnoreCase)) is not InProcessWebsite site)
            {
                continue;
            }

            var hostnames = siteHostname.Hostname?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
            if (hostnames.Length == 0)
            {
                continue;
            }

            if (hostnames.Length == site.Hosts.Count)
            {
                var identical = true;
                for (var i = 0; i < hostnames.Length; i++)
                {
                    identical = i == 0
                        ? hostnames[i].Equals(site.Hosts[i].Authority, StringComparison.OrdinalIgnoreCase) && site.Hosts[i].Type == ApplicationHostType.Primary
                        : hostnames[i].Equals(site.Hosts[i].Authority, StringComparison.OrdinalIgnoreCase);
                }

                if (identical)
                {
                    continue;
                }
            }

            if (site.CreateWritableClone() is not InProcessWebsite writableSite)
            {
                continue;
            }
            site.Hosts.Clear();

            for (var i = 0; i < hostnames.Length; i++)
            {
                site.Hosts.Add(new ApplicationHost(hostnames[i])
                {
                    Type = i == 0 ? ApplicationHostType.Primary : ApplicationHostType.Default,
                });
            }
            _applicationRepository.SaveAsync(writableSite).GetAwaiter().GetResult();
        }
    }
}
