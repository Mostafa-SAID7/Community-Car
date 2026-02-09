using CommunityCar.Domain.DTOs.Communications;
using CommunityCar.Domain.Enums.Community.friends;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Communications;
using CommunityCar.Domain.Interfaces.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace CommunityCar.Web.Areas.Communications.Controllers;

[Area("Communications")]
[Route("{culture:alpha}/Communications/[controller]")]
[Route("Communications/[controller]")]
[Authorize]
public class ChatsController : Controller
{
    private readonly IChatService _chatService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFriendshipService _friendshipService;
    private readonly ILogger<ChatsController> _logger;
    private readonly IStringLocalizer<ChatsController> _localizer;

    public ChatsController(
        IChatService chatService,
        ICurrentUserService currentUserService,
        IFriendshipService friendshipService,
        ILogger<ChatsController> logger,
        IStringLocalizer<ChatsController> localizer)
    {
        _chatService = chatService;
        _currentUserService = currentUserService;
        _friendshipService = friendshipService;
        _logger = logger;
        _localizer = localizer;
    }

    // GET: Chats
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetCurrentUserId();
            var conversations = await _chatService.GetConversationsAsync(userId);
            var unreadCount = await _chatService.GetUnreadCountAsync(userId);
            var friends = await _friendshipService.GetFriendsAsync(userId);

            ViewBag.UnreadCount = unreadCount;
            ViewBag.Friends = friends;
            return View(conversations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading chat conversations");
            TempData["Error"] = _localizer["FailedToLoadConversations"].Value;
            return View(new List<ChatConversationDto>());
        }
    }

    // GET: Chats/Conversation/{id}
    [HttpGet("Conversation/{id:guid}")]
    public async Task<IActionResult> Conversation(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            if (id == currentUserId)
            {
                TempData["Error"] = _localizer["CannotChatWithSelf"].Value;
                return RedirectToAction(nameof(Index));
            }

            // Verify friendship before allowing chat
            var friendshipStatus = await _friendshipService.GetFriendshipStatusAsync(currentUserId, id);
            if (friendshipStatus != FriendshipStatus.Accepted)
            {
                TempData["Error"] = _localizer["OnlyAcceptedFriendsCanChat"].Value;
                return RedirectToAction("Index", "Friends");
            }

            var messages = await _chatService.GetMessagesAsync(currentUserId, id);
            var conversations = await _chatService.GetConversationsAsync(currentUserId);

            // Mark conversation as read
            await _chatService.MarkConversationAsReadAsync(currentUserId, id);

            ViewBag.OtherUserId = id;
            ViewBag.Conversations = conversations;

            return View(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading conversation with user {UserId}", id);
            TempData["Error"] = _localizer["FailedToLoadConversation"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Chats/SendMessage
    [HttpPost("SendMessage")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMessage([FromForm] SendMessageDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = _localizer["InvalidMessageData"].Value });
            }

            var senderId = GetCurrentUserId();
            var message = await _chatService.SendMessageAsync(senderId, dto);

            return Json(new { success = true, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return Json(new { success = false, message = _localizer["FailedToSendMessage"].Value });
        }
    }

    // GET: Chats/GetMessages/{id}
    [HttpGet("GetMessages/{id:guid}")]
    public async Task<IActionResult> GetMessages(Guid id, int page = 1, int pageSize = 50)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var messages = await _chatService.GetMessagesAsync(currentUserId, id, page, pageSize);

            return Json(new { success = true, messages });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting messages");
            return Json(new { success = false, message = _localizer["FailedToLoadMessages"].Value });
        }
    }

    // POST: Chats/MarkAsRead/{messageId}
    [HttpPost("MarkAsRead/{messageId:guid}")]
    public async Task<IActionResult> MarkAsRead(Guid messageId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _chatService.MarkAsReadAsync(messageId, userId);

            return Json(new { success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message as read");
            return Json(new { success = false });
        }
    }

    // POST: Chats/MarkConversationAsRead/{id}
    [HttpPost("MarkConversationAsRead/{id:guid}")]
    public async Task<IActionResult> MarkConversationAsRead(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _chatService.MarkConversationAsReadAsync(currentUserId, id);

            return Json(new { success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking conversation as read");
            return Json(new { success = false });
        }
    }

    // DELETE: Chats/DeleteMessage/{messageId}
    [HttpPost("DeleteMessage/{messageId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _chatService.DeleteMessageAsync(messageId, userId);

            if (result)
            {
                return Json(new { success = true, message = _localizer["MessageDeleted"].Value });
            }

            return Json(new { success = false, message = _localizer["FailedToDeleteMessage"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message");
            return Json(new { success = false, message = _localizer["FailedToDeleteMessage"].Value });
        }
    }

    // GET: Chats/GetUnreadCount
    [HttpGet("GetUnreadCount")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _chatService.GetUnreadCountAsync(userId);

            return Json(new { success = true, count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return Json(new { success = false, count = 0 });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException(_localizer["UserNotAuthenticated"]);
        }

        return userId;
    }
}
