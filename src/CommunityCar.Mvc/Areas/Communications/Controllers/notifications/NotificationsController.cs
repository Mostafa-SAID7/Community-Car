using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Interfaces.Communications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Communications.Controllers.notifications;

[Area("Communications")]
[Route("Communications/[controller]")]
[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationsController(INotificationService notificationService, UserManager<ApplicationUser> userManager)
    {
        _notificationService = notificationService;
        _userManager = userManager;
    }

    public IActionResult Index() => View();

    [HttpGet("UnreadCount")]
    public async Task<IActionResult> UnreadCount()
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Json(new { count });
    }

    [HttpGet("Latest")]
    public async Task<IActionResult> Latest()
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        
        var result = notifications.Select(n => new
        {
            id = n.Id,
            title = n.Title,
            message = n.Message,
            link = n.Link,
            createdAt = n.CreatedAt.ToString("g"),
            isRead = n.IsRead
        });

        return Json(result);
    }

    [HttpPost("MarkAsRead/{id}")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok();
    }

    [HttpPost("MarkAllAsRead")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok();
    }
}
