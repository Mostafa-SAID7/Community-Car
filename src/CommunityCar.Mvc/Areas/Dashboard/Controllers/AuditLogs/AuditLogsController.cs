using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.AuditLogs;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("Dashboard/[controller]")]
public class AuditLogsController : Controller
{
    public IActionResult Index() => View();
}
