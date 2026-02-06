using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.settings;

[Area("Dashboard")]
[Route("Dashboard/[controller]")]
public class SettingsController : Controller
{
    public IActionResult Index() => View();
}
