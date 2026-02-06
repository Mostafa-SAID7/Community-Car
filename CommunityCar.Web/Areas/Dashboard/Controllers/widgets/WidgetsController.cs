using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.widgets;

[Area("Dashboard")]
[Route("Dashboard/[controller]")]
public class WidgetsController : Controller
{
    public IActionResult Index() => View();
}
