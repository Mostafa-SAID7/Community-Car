using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Identity.Controllers.Profiles;

[Area("Identity")]
[Route("Identity/[controller]")]
public class ProfilesController : Controller
{
    public IActionResult Index() => View();
}
