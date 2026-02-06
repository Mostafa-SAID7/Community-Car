using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Community.Controllers.feed;

[Area("Community")]
[Route("Community/[controller]")]
public class FeedController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
