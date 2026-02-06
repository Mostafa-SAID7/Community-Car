using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.management;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ContentManagementController : Controller
{
    public IActionResult Index() => View();
}
