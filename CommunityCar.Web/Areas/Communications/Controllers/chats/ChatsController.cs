using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Communications.Controllers.chats;

[Area("Communications")]
[Route("Communications/[controller]")]
public class ChatsController : Controller
{
    public IActionResult Index() => View();
}
