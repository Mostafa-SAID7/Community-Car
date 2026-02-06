using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.reports;

[Area("Dashboard")]
[Route("Dashboard/[controller]")]
public class ReportsController : Controller
{
    public IActionResult Index() => View();
}
