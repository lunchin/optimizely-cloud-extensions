using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions;

public class SiteHostnameInitilization : IBlockingFirstRequestInitializer
{
    private const string Preproduction = "Preproduction";
    private const string Integration = "Integration";
    private readonly ISiteDefinitionRepository _siteDefinitionRepository;
    private readonly ExtensionsOptions _extensionsOptions;
    private readonly string _environment;

    public SiteHostnameInitilization(ISiteDefinitionRepository siteDefinitionRepository,
                            IOptions<ExtensionsOptions> extensionsOptions)
    {
        _siteDefinitionRepository = siteDefinitionRepository;
        _extensionsOptions = extensionsOptions.Value;
        _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
    }

    public bool CanRunInParallel => false;

    public async Task InitializeAsync(HttpContext httpContext)
    {
        if (!_extensionsOptions.SiteHostnameInitilizationEnabled)
        {
            return;
        }

        UpdateSiteHostnames(httpContext);
        await Task.CompletedTask;
    }

    private void UpdateSiteHostnames(HttpContext context)
    {
        if (!_environment.Equals(Environments.Development) && !_environment.Equals(Integration) && !_environment.Equals(Preproduction))
        {
            return;
        }

        var sites = _siteDefinitionRepository.List();
        if (!sites?.Any() ?? true)
        {
            return;
        }

        var request = context.Request;
        foreach (var siteHostname in _extensionsOptions.Sites)
        {
            var site = sites?.FirstOrDefault(x => x.Name.Equals(siteHostname.Name, StringComparison.OrdinalIgnoreCase));
            if (site == null)
            {
                continue;
            }

            var hostnames = siteHostname.Hostname?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            if (!hostnames.Any())
            {
                continue;
            }

            if (hostnames.Length == site.Hosts.Count)
            {
                var identical = true;
                for (var i = 0; i < hostnames.Length; i++)
                {
                    identical = i == 0
                        ? hostnames[i].Equals(site.Hosts[i].Name, StringComparison.OrdinalIgnoreCase) && site.Hosts[i].Type == HostDefinitionType.Primary
                        : hostnames[i].Equals(site.Hosts[i].Name, StringComparison.OrdinalIgnoreCase);
                }

                if (identical)
                {
                    continue;
                }
            }

            site = site.CreateWritableClone();
            site.SiteUrl = new Uri($"https://{hostnames[0]}");
            site.Hosts.Clear();

            for (var i = 0; i < hostnames.Length; i++)
            {
                site.Hosts.Add(new HostDefinition
                {
                    Name = hostnames[i],
                    Type = i == 0 ? HostDefinitionType.Primary : HostDefinitionType.Undefined,
                });
            }
            _siteDefinitionRepository.Save(site);
        }
    }
}
