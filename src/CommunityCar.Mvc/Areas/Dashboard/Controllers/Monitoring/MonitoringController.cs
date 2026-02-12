using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Mvc.Areas.Dashboard.Controllers.Monitoring;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Monitoring")]
public class MonitoringController : Controller
{
    [HttpGet("")]
    public IActionResult Index(string? tab = "health")
    {
        ViewData["ActiveTab"] = tab?.ToLower() ?? "health";
        return View();
    }
}
