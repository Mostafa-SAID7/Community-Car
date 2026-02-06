using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.UserActivity;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class UserActivityController : Controller
{
    public IActionResult Index() => View();
}
