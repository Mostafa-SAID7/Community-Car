using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.system;

[Area("Dashboard")]
[Route("Dashboard/[controller]")]
public class SystemController : Controller
{
    public IActionResult Index() => View();
}
