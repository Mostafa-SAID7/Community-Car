using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.localization;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("Dashboard/[controller]")]
public class LocalizationController : Controller
{
    [HttpGet("")]
    [HttpGet("{part}")]
    public IActionResult Index(string part = "Main")
    {
        ViewBag.CurrentPart = part;
        return View();
    }
}
