using EPiServer.Shell.ViewComposition;
using Microsoft.Extensions.Options;

namespace lunchin.Optimizely.Cloud.Extensions.UI;

[Component]
public class EnvironmentHighlighterComponent : ComponentDefinitionBase
{
    private readonly ExtensionsOptions? _options;

    public EnvironmentHighlighterComponent()
           : base("loce/widgets/environmentHighlighterWidget")
    {
        Categories = new[] { "cms" };
        _options = ServiceLocator.Current.GetInstance<IOptions<ExtensionsOptions>>().Value;
        if (_options?.EnvironmentBannerEnabled ?? false)
        {
            PlugInAreas = new[] { "/episerver/cms/action" };
        }
    }

    public override IComponent CreateComponent()
    {

        var component = base.CreateComponent();
        component.Settings.Add("environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        component.Settings.Add("backgroundColor", _options?.EnvironmentBannerBaackgroundColor ?? "#2cd31f");
        component.Settings.Add("textColor", _options?.EnvironmentBannerTextColor ?? "ffffff");
        return component;
    }
}
