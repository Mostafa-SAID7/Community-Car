using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Community.Controllers.reviews;

[Area("Community")]
[Route("Community/[controller]")]
public class ReviewsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
