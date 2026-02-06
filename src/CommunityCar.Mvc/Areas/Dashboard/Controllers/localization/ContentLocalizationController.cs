using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.localization;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ContentLocalizationController : Controller
{
    public IActionResult Index() => View();
}
