using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.reports;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class UserReportsController : Controller
{
    public IActionResult Index() => View();
}
