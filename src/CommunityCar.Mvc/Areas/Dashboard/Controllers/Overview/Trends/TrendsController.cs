using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.Overview.Trends;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Overview/Trends")]
public class TrendsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
