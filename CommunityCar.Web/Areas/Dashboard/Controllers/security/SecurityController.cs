using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.security;

[Area("Dashboard")]
[Route("Dashboard/[controller]")]
public class SecurityController : Controller
{
    public IActionResult Index() => View();
}
