using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Controllers.Community;

[Route("[controller]")]
public class EventsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
