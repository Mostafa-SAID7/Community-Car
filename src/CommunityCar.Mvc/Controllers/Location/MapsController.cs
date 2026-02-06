using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Controllers.Location;

[Route("[controller]")]
public class MapsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
