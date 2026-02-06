using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Communications.Controllers.notifications;

[Area("Communications")]
[Route("Communications/[controller]")]
public class NotificationsController : Controller
{
    public IActionResult Index() => View();
}
