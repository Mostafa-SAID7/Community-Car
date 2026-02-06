using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Controllers.Content;

[Route("[controller]")]
public class NewsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
