using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Community.Controllers.guides;

[Area("Community")]
[Route("Community/[controller]")]
public class GuidesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
