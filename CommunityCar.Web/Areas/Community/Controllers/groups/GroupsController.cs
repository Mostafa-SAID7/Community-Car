using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Community.Controllers.groups;

[Area("Community")]
[Route("Community/[controller]")]
public class GroupsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
