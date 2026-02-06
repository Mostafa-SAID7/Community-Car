using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.analytics;

[Area("Dashboard")]
[Route("Dashboard/[controller]")]
public class AnalyticsController : Controller
{
    public IActionResult Index() => View();
}
