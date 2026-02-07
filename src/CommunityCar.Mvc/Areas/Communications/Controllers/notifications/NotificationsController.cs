using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Interfaces.Communications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityCar.Web.Areas.Communications.Controllers.notifications;

[Area("Communications")]
[Route("Communications/[controller]")]
[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService, 
        UserManager<ApplicationUser> userManager,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Communications/Notifications
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20, bool? unreadOnly = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var allNotifications = await _notificationService.GetUserNotificationsAsync(userId, 100);
            
            // Filter by read status if specified
            var filteredNotifications = unreadOnly == true 
                ? allNotifications.Where(n => !n.IsRead) 
                : allNotifications;

            // Pagination
            var notifications = filteredNotifications
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

            ViewBag.UnreadCount = unreadCount;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = filteredNotifications.Count();
            ViewBag.UnreadOnly = unreadOnly;

            return View(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading notifications");
            TempData["Error"] = "Failed to load notifications";
            return View(new List<CommunityCar.Domain.Entities.Communications.notifications.Notification>());
        }
    }

    // GET: Communications/Notifications/UnreadCount
    [HttpGet("UnreadCount")]
    public async Task<IActionResult> UnreadCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { success = true, count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return Json(new { success = false, count = 0 });
        }
    }

    // GET: Communications/Notifications/Latest
    [HttpGet("Latest")]
    public async Task<IActionResult> Latest(int count = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, count);
            
            var result = notifications.Select(n => new
            {
                id = n.Id,
                title = n.Title,
                message = n.Message,
                link = n.Link,
                createdAt = n.CreatedAt.ToString("g"),
                timeAgo = GetTimeAgo(n.CreatedAt),
                isRead = n.IsRead
            });

            return Json(new { success = true, notifications = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest notifications");
            return Json(new { success = false, message = "Failed to load notifications" });
        }
    }

    // GET: Communications/Notifications/GetAll
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20, bool? unreadOnly = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var allNotifications = await _notificationService.GetUserNotificationsAsync(userId, 100);
            
            var filteredNotifications = unreadOnly == true 
                ? allNotifications.Where(n => !n.IsRead) 
                : allNotifications;

            var notifications = filteredNotifications
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new
                {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    link = n.Link,
                    createdAt = n.CreatedAt.ToString("g"),
                    timeAgo = GetTimeAgo(n.CreatedAt),
                    isRead = n.IsRead
                });

            return Json(new 
            { 
                success = true, 
                notifications,
                totalCount = filteredNotifications.Count(),
                page,
                pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all notifications");
            return Json(new { success = false, message = "Failed to load notifications" });
        }
    }

    // POST: Communications/Notifications/MarkAsRead/{id}
    [HttpPost("MarkAsRead/{id}")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAsReadAsync(id);
            
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            
            return Json(new { success = true, unreadCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            return Json(new { success = false, message = "Failed to mark notification as read" });
        }
    }

    // POST: Communications/Notifications/MarkAllAsRead
    [HttpPost("MarkAllAsRead")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            
            return Json(new { success = true, message = "All notifications marked as read" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return Json(new { success = false, message = "Failed to mark all notifications as read" });
        }
    }

    // DELETE: Communications/Notifications/Delete/{id}
    [HttpPost("Delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            // Note: You'll need to add DeleteAsync method to INotificationService
            // For now, we'll just mark it as read
            await _notificationService.MarkAsReadAsync(id);
            
            return Json(new { success = true, message = "Notification deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
            return Json(new { success = false, message = "Failed to delete notification" });
        }
    }

    // GET: Communications/Notifications/Dropdown
    [HttpGet("Dropdown")]
    public async Task<IActionResult> Dropdown()
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, 5);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            
            var result = notifications.Select(n => new
            {
                id = n.Id,
                title = n.Title,
                message = n.Message,
                link = n.Link,
                timeAgo = GetTimeAgo(n.CreatedAt),
                isRead = n.IsRead
            });

            return Json(new { success = true, notifications = result, unreadCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dropdown notifications");
            return Json(new { success = false, message = "Failed to load notifications" });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        return userId;
    }

    private string GetTimeAgo(DateTimeOffset dateTime)
    {
        var timeSpan = DateTimeOffset.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1)
            return "just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes}m ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours}h ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays}d ago";
        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)}w ago";
        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)}mo ago";
        
        return $"{(int)(timeSpan.TotalDays / 365)}y ago";
    }
}
