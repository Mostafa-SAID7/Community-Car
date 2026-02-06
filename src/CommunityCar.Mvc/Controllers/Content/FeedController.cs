using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Controllers.Content;

[Route("[controller]")]
public class FeedController : Controller
{
    [HttpGet("~/")]
    [Route("~/")]
    public IActionResult Index()
    {
        return View();
    }
}
