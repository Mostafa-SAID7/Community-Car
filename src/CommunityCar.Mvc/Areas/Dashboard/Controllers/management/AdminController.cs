using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.management;

[Area("Dashboard")]
[Route("Dashboard/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : Controller
{
    public IActionResult Index() => View();
    
    public IActionResult Settings() => View();
}
