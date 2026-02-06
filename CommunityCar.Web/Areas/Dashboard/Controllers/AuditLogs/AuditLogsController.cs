using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.AuditLogs;

[Area("Dashboard")]
[Route("Dashboard/[controller]")]
public class AuditLogsController : Controller
{
    public IActionResult Index() => View();
}
