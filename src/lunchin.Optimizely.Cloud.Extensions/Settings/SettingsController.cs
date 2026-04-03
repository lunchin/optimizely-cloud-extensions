using EPiServer.Shell.ViewComposition;
using EPiServer.Shell.Web.Mvc;

namespace lunchin.Optimizely.Cloud.Extensions.Settings;

public class SettingsController(IBootstrapper bootstrapper, IViewManager viewManager) : Controller
{
    private readonly IBootstrapper _bootstrapper = bootstrapper;
    private readonly IViewManager _viewManager = viewManager;

    public SettingsController()
        : this(ServiceLocator.Current.GetInstance<IBootstrapper>(), ServiceLocator.Current.GetInstance<IViewManager>())
    {
    }

    public ActionResult Index(ShellModule module, string controller)
    {
        var view = _viewManager.GetView(module, controller);
        var viewModel = _bootstrapper.CreateViewModel(view.Name, ControllerContext, module.Name);

        return View(_bootstrapper.BootstrapperViewName, viewModel);
    }
}
