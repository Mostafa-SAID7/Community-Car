using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Identity.Controllers.Permissions;

[Area("Identity")]
[Route("Identity/[controller]")]
public class PermissionsController : Controller
{
    public IActionResult Index() => View();
}
