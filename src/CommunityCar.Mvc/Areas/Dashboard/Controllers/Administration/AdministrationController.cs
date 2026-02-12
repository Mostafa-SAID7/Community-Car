using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Mvc.Areas.Dashboard.Controllers.Administration;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Administration")]
public class AdministrationController : Controller
{
    [HttpGet("")]
    public IActionResult Index(string? tab = "localization")
    {
        ViewData["ActiveTab"] = tab?.ToLower() ?? "localization";
        return View();
    }
}
