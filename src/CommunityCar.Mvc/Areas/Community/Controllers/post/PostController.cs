using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Community.Controllers.post;

[Area("Community")]
[Route("Community/[controller]")]
public class PostController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
