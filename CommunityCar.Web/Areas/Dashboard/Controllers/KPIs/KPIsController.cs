using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.KPIs;

[Area("Dashboard")]
[Route("Dashboard/[controller]")]
public class KPIsController : Controller
{
    public IActionResult Index() => View();
}
