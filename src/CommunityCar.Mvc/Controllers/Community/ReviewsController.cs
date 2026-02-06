using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Controllers.Community;

[Route("[controller]")]
public class ReviewsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
