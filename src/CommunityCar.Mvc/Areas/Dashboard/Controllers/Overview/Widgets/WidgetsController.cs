using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.Overview.Widgets;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Overview/Widgets")]
public class WidgetsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
