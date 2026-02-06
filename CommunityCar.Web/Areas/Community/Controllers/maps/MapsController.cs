using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Community.Controllers.maps;

[Area("Community")]
[Route("Community/[controller]")]
public class MapsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
