using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Controllers.Content;

[Route("[controller]")]
public class GuidesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
