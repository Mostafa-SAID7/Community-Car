using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.analytics;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class UserAnalyticsController : Controller
{
    public IActionResult Index() => View();
}
