using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Community.Controllers.events;

[Area("Community")]
[Route("Community/[controller]")]
public class EventsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
