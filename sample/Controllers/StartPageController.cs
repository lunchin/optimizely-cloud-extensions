using sample.Models.Pages;
using sample.Models.ViewModels;

namespace sample.Controllers;

public class StartPageController : PageControllerBase<StartPage>
{
    public IActionResult Index(StartPage currentPage)
    {
        var model = PageViewModel.Create(currentPage);
        return View(model);
    }
}
