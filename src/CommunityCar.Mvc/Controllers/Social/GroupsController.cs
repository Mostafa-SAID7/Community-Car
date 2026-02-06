using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Controllers.Social;

[Route("[controller]")]
public class GroupsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
