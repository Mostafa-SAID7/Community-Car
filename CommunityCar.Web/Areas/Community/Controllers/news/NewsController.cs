using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Community.Controllers.news;

[Area("Community")]
[Route("Community/[controller]")]
public class NewsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
