using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.management;

[Area("Dashboard")]
[Route("Dashboard/[controller]")]
public class ManagementController : Controller
{
    public IActionResult Index() => View();
}
