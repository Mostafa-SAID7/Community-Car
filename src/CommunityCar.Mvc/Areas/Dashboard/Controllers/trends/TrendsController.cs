using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.trends;

[Area("Dashboard")]
[Route("Dashboard/[controller]")]
public class TrendsController : Controller
{
    public IActionResult Index() => View();
}
