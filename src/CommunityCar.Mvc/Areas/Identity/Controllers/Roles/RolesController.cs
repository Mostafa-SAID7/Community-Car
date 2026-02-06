using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Identity.Controllers.Roles;

[Area("Identity")]
[Route("Identity/[controller]")]
public class RolesController : Controller
{
    public IActionResult Index() => View();
}
