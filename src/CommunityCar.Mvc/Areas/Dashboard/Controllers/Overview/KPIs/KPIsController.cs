using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.Overview.KPIs;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Overview/KPIs")]
public class KPIsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
