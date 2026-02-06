using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.health;

[Area("Dashboard")]
[Route("Dashboard/[controller]")]
public class HealthController : Controller
{
    public IActionResult Index() => View();
}
