using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Community.Controllers.qa;

[Area("Community")]
[Route("Community/[controller]")]
public class QAController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
