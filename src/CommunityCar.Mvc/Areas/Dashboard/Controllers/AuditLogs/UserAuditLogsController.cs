using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.AuditLogs;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class UserAuditLogsController : Controller
{
    public IActionResult Index() => View();
}
