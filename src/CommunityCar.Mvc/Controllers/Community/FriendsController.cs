using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Web.ViewModels.Community;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace CommunityCar.Mvc.Controllers.Community;

[Route("{culture:alpha}/[controller]")]
[Authorize]
public class FriendsController : Controller
{
    private readonly IFriendshipService _friendshipService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<FriendsController> _logger;
    private readonly CommunityCar.Domain.Interfaces.Communications.INotificationService _notificationService;
    private readonly IStringLocalizer<FriendsController> _localizer;
    private readonly IHubContext<CommunityCar.Infrastructure.Hubs.FriendHub> _friendHubContext;

    public FriendsController(
        IFriendshipService friendshipService, 
        UserManager<ApplicationUser> userManager,
        ILogger<FriendsController> logger,
        CommunityCar.Domain.Interfaces.Communications.INotificationService notificationService,
        IStringLocalizer<FriendsController> localizer,
        IHubContext<CommunityCar.Infrastructure.Hubs.FriendHub> friendHubContext)
    {
        _friendshipService = friendshipService;
        _userManager = userManager;
        _logger = logger;
        _notificationService = notificationService;
        _localizer = localizer;
        _friendHubContext = friendHubContext;
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetCurrentUserId();
            var friendships = await _friendshipService.GetFriendsAsync(userId);
            
            // Map friendships correctly regardless of which side the current user is on
            var viewModels = friendships.Select(f =>
            {
                // Determine the actual friend (the other user in the relationship)
                var friendUser = f.UserId == userId ? f.Friend : f.User;
                var friendId = f.UserId == userId ? f.FriendId : f.UserId;
                
                return new FriendshipViewModel
                {
                    Id = f.Id,
                    FriendId = friendId,
                    Slug = friendUser?.Slug ?? string.Empty,
                    FriendName = $"{friendUser?.FirstName ?? "Unknown"} {friendUser?.LastName ?? "User"}",
                    ProfilePictureUrl = friendUser?.ProfilePictureUrl,
                    Status = f.Status,
                    Since = f.CreatedAt
                };
            }).ToList();

            var requests = await _friendshipService.GetPendingRequestsAsync(userId);
            ViewData["FriendCount"] = viewModels.Count;
            ViewData["RequestCount"] = requests.Count();
            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading friends list");
            TempData["Error"] = _localizer["FailedToLoadFriendsList"].Value;
            return View(new List<FriendshipViewModel>());
        }
    }

    [HttpGet("GetPendingRequestCount")]
    [ResponseCache(Duration = 10, VaryByHeader = "Cookie")]
    public async Task<IActionResult> GetPendingRequestCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _friendshipService.GetPendingRequestsCountAsync(userId);
            return Json(new { success = true, count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending request count");
            return Json(new { success = false, count = 0 });
        }
    }

    [HttpGet("Requests")]
    public async Task<IActionResult> Requests()
    {
        try
        {
            var userId = GetCurrentUserId();
            var requests = await _friendshipService.GetPendingRequestsAsync(userId);
            
            var viewModels = requests.Select(r => {
                // Log if UserId is empty
                if (r.UserId == Guid.Empty)
                {
                    _logger.LogWarning("Found friendship record with empty UserId for FriendId: {FriendId}", userId);
                }
                
                return new FriendRequestViewModel
                {
                    UserId = r.UserId, // This is the person who sent the request
                    UserName = $"{r.User?.FirstName ?? "Unknown"} {r.User?.LastName ?? "User"}",
                    ProfilePictureUrl = r.User?.ProfilePictureUrl,
                    Slug = r.User?.Slug ?? string.Empty,
                    ReceivedAt = r.CreatedAt
                };
            }).Where(vm => vm.UserId != Guid.Empty) // Filter out invalid records
            .ToList();

            var friendships = await _friendshipService.GetFriendsAsync(userId);
            ViewData["FriendCount"] = friendships.Count();
            ViewData["RequestCount"] = viewModels.Count;
            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading friend requests");
            TempData["Error"] = _localizer["FailedToLoadFriendRequests"].Value;
            return View(new List<FriendRequestViewModel>());
        }
    }

    [HttpGet("SentRequests")]
    public async Task<IActionResult> SentRequests()
    {
        try
        {
            var userId = GetCurrentUserId();
            var sentRequests = await _friendshipService.GetSentRequestsAsync(userId);
            
            var viewModels = sentRequests.Select(r => new FriendRequestViewModel
            {
                UserId = r.FriendId,
                UserName = $"{r.Friend?.FirstName ?? "Unknown"} {r.Friend?.LastName ?? "User"}",
                ProfilePictureUrl = r.Friend?.ProfilePictureUrl,
                Slug = r.Friend?.Slug ?? string.Empty,
                ReceivedAt = r.CreatedAt
            }).ToList();

            var friendships = await _friendshipService.GetFriendsAsync(userId);
            var requests = await _friendshipService.GetPendingRequestsAsync(userId);
            ViewData["FriendCount"] = friendships.Count();
            ViewData["RequestCount"] = requests.Count();
            ViewData["SentRequestCount"] = viewModels.Count;
            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading sent friend requests");
            TempData["Error"] = _localizer["FailedToLoadSentRequests"].Value;
            return View(new List<FriendRequestViewModel>());
        }
    }

    [HttpPost("SendRequest")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendRequest(Guid friendId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            if (userId == friendId)
            {
                TempData["Error"] = _localizer["CannotSendRequestToSelf"].Value;
                return RedirectToAction(nameof(Index));
            }

            // Check if target user is an admin
            var targetUser = await _userManager.FindByIdAsync(friendId.ToString());
            if (targetUser != null)
            {
                var isAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");
                var isSuperAdmin = await _userManager.IsInRoleAsync(targetUser, "SuperAdmin");
                
                if (isAdmin || isSuperAdmin)
                {
                    TempData["Error"] = _localizer["CannotSendRequestToAdmin"].Value ?? "Cannot send friend requests to administrators.";
                    return RedirectToAction(nameof(Index));
                }
            }

            await _friendshipService.SendRequestAsync(userId, friendId);
            
            // Notify user via database notification
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserName = $"{currentUser?.FirstName ?? "Unknown"} {currentUser?.LastName ?? "User"}";
            await _notificationService.NotifyUserOfFriendRequestAsync(friendId, userId, currentUserName);

            // Send real-time notification via SignalR
            var connectionId = CommunityCar.Infrastructure.Hubs.FriendHub.GetConnectionId(friendId);
            if (connectionId != null)
            {
                await _friendHubContext.Clients.Client(connectionId).SendAsync("ReceiveFriendRequest", new
                {
                    SenderId = userId,
                    SenderName = currentUserName,
                    SenderProfilePicture = currentUser?.ProfilePictureUrl,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            TempData["Success"] = _localizer["FriendRequestSent"].Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend request to {FriendId}", friendId);
            TempData["Error"] = _localizer["FailedToSendRequest"].Value;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("SendRequestJson")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendRequestJson(Guid friendId)
    {
        try
        {
            // Validate friendId
            if (friendId == Guid.Empty)
            {
                return Json(new { success = false, message = _localizer["InvalidFriendId"].Value });
            }

            var userId = GetCurrentUserId();
            
            if (userId == friendId)
            {
                return Json(new { success = false, message = _localizer["CannotSendRequestToSelf"].Value });
            }

            // Check if target user is an admin
            var targetUser = await _userManager.FindByIdAsync(friendId.ToString());
            if (targetUser != null)
            {
                var isAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");
                var isSuperAdmin = await _userManager.IsInRoleAsync(targetUser, "SuperAdmin");
                
                if (isAdmin || isSuperAdmin)
                {
                    return Json(new { success = false, message = _localizer["CannotSendRequestToAdmin"].Value ?? "Cannot send friend requests to administrators." });
                }
            }

            await _friendshipService.SendRequestAsync(userId, friendId);
            
            // Notify user via database notification
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserName = $"{currentUser?.FirstName ?? "Unknown"} {currentUser?.LastName ?? "User"}";
            await _notificationService.NotifyUserOfFriendRequestAsync(friendId, userId, currentUserName);

            // Send real-time notification via SignalR
            var connectionId = CommunityCar.Infrastructure.Hubs.FriendHub.GetConnectionId(friendId);
            if (connectionId != null)
            {
                await _friendHubContext.Clients.Client(connectionId).SendAsync("ReceiveFriendRequest", new
                {
                    SenderId = userId,
                    SenderName = currentUserName,
                    SenderProfilePicture = currentUser?.ProfilePictureUrl,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            return Json(new { success = true, message = "Friend request sent successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend request to {FriendId}", friendId);
            return Json(new { success = false, message = _localizer["FailedToSendRequest"].Value });
        }
    }

    [HttpPost("AcceptRequest")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptRequest(Guid friendId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _friendshipService.AcceptRequestAsync(userId, friendId);
            
            // Notify user who sent the request via database notification
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserName = $"{currentUser?.FirstName ?? "Unknown"} {currentUser?.LastName ?? "User"}";
            await _notificationService.NotifyUserOfFriendRequestAcceptedAsync(friendId, userId, currentUserName);

            // Send real-time notification via SignalR
            var connectionId = CommunityCar.Infrastructure.Hubs.FriendHub.GetConnectionId(friendId);
            if (connectionId != null)
            {
                await _friendHubContext.Clients.Client(connectionId).SendAsync("FriendRequestAccepted", new
                {
                    FriendId = userId,
                    FriendName = currentUserName,
                    FriendProfilePicture = currentUser?.ProfilePictureUrl,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            TempData["Success"] = _localizer["RequestAccepted"].Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting friend request from {FriendId}", friendId);
            TempData["Error"] = _localizer["FailedToAcceptRequest"].Value;
        }

        return RedirectToAction(nameof(Requests));
    }
    [HttpGet("AcceptRequest")]
    public IActionResult AcceptRequestGet(Guid? friendId)
    {
        // GET requests should redirect to the Requests page
        // The actual accept action requires POST for security
        if (friendId.HasValue)
        {
            TempData["Info"] = _localizer["PleaseUseAcceptButton"].Value;
        }
        return RedirectToAction(nameof(Requests));
    }


    [HttpPost("AcceptRequestJson")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptRequestJson(Guid friendId)
    {
        try
        {
            // Validate friendId
            if (friendId == Guid.Empty)
            {
                return Json(new { success = false, message = _localizer["InvalidFriendId"].Value });
            }

            var userId = GetCurrentUserId();
            await _friendshipService.AcceptRequestAsync(userId, friendId);
            
            // Notify user who sent the request via database notification
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserName = $"{currentUser?.FirstName ?? "Unknown"} {currentUser?.LastName ?? "User"}";
            
            // Only send notification if friendId is valid
            if (friendId != Guid.Empty)
            {
                await _notificationService.NotifyUserOfFriendRequestAcceptedAsync(friendId, userId, currentUserName);
            }

            // Send real-time notification via SignalR
            var connectionId = CommunityCar.Infrastructure.Hubs.FriendHub.GetConnectionId(friendId);
            if (connectionId != null)
            {
                await _friendHubContext.Clients.Client(connectionId).SendAsync("FriendRequestAccepted", new
                {
                    FriendId = userId,
                    FriendName = currentUserName,
                    FriendProfilePicture = currentUser?.ProfilePictureUrl,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            return Json(new { success = true, message = "Friend request accepted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting friend request from {FriendId}", friendId);
            return Json(new { success = false, message = _localizer["FailedToAcceptRequest"].Value });
        }
    }

    [HttpPost("RejectRequest")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectRequest(Guid friendId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _friendshipService.RejectRequestAsync(userId, friendId);
            
            // Send real-time notification via SignalR
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserName = $"{currentUser?.FirstName ?? "Unknown"} {currentUser?.LastName ?? "User"}";
            var connectionId = CommunityCar.Infrastructure.Hubs.FriendHub.GetConnectionId(friendId);
            if (connectionId != null)
            {
                await _friendHubContext.Clients.Client(connectionId).SendAsync("FriendRequestRejected", new
                {
                    UserId = userId,
                    UserName = currentUserName,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            TempData["Success"] = _localizer["RequestRejected"].Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting friend request from {FriendId}", friendId);
            TempData["Error"] = _localizer["FailedToRejectRequest"].Value;
        }

        return RedirectToAction(nameof(Requests));
    }

    [HttpPost("RejectRequestJson")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectRequestJson(Guid friendId)
    {
        try
        {
            // Validate friendId
            if (friendId == Guid.Empty)
            {
                return Json(new { success = false, message = _localizer["InvalidFriendId"].Value });
            }

            var userId = GetCurrentUserId();
            await _friendshipService.RejectRequestAsync(userId, friendId);
            
            // Send real-time notification via SignalR
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserName = $"{currentUser?.FirstName ?? "Unknown"} {currentUser?.LastName ?? "User"}";
            var connectionId = CommunityCar.Infrastructure.Hubs.FriendHub.GetConnectionId(friendId);
            if (connectionId != null)
            {
                await _friendHubContext.Clients.Client(connectionId).SendAsync("FriendRequestRejected", new
                {
                    UserId = userId,
                    UserName = currentUserName,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            return Json(new { success = true, message = _localizer["RequestRejected"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting friend request from {FriendId}", friendId);
            return Json(new { success = false, message = _localizer["FailedToRejectRequest"].Value });
        }
    }

    [HttpPost("RemoveFriend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFriend(Guid friendId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _friendshipService.RemoveFriendAsync(userId, friendId);
            
            // Send real-time notification via SignalR
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserName = $"{currentUser?.FirstName ?? "Unknown"} {currentUser?.LastName ?? "User"}";
            var connectionId = CommunityCar.Infrastructure.Hubs.FriendHub.GetConnectionId(friendId);
            if (connectionId != null)
            {
                await _friendHubContext.Clients.Client(connectionId).SendAsync("FriendshipRemoved", new
                {
                    RemovedBy = userId,
                    RemovedByName = currentUserName,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            TempData["Success"] = _localizer["FriendRemoved"].Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing friend {FriendId}", friendId);
            TempData["Error"] = _localizer["FailedToRemoveFriend"].Value;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("RemoveFriendJson")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFriendJson(Guid friendId)
    {
        try
        {
            // Validate friendId
            if (friendId == Guid.Empty)
            {
                return Json(new { success = false, message = _localizer["InvalidFriendId"].Value });
            }

            var userId = GetCurrentUserId();
            await _friendshipService.RemoveFriendAsync(userId, friendId);
            
            // Send real-time notification via SignalR
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserName = $"{currentUser?.FirstName ?? "Unknown"} {currentUser?.LastName ?? "User"}";
            var connectionId = CommunityCar.Infrastructure.Hubs.FriendHub.GetConnectionId(friendId);
            if (connectionId != null)
            {
                await _friendHubContext.Clients.Client(connectionId).SendAsync("FriendshipRemoved", new
                {
                    RemovedBy = userId,
                    RemovedByName = currentUserName,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            return Json(new { success = true, message = _localizer["FriendRemoved"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing friend {FriendId}", friendId);
            return Json(new { success = false, message = _localizer["FailedToRemoveFriend"].Value });
        }
    }

    [HttpPost("BlockUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BlockUser(Guid friendId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            if (userId == friendId)
            {
                TempData["Error"] = _localizer["CannotBlockSelf"].Value;
                return RedirectToAction(nameof(Index));
            }

            // Check if target user is an admin
            var targetUser = await _userManager.FindByIdAsync(friendId.ToString());
            if (targetUser != null)
            {
                var isAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");
                var isSuperAdmin = await _userManager.IsInRoleAsync(targetUser, "SuperAdmin");
                
                if (isAdmin || isSuperAdmin)
                {
                    TempData["Error"] = _localizer["CannotBlockAdmin"].Value ?? "Cannot block administrators.";
                    return RedirectToAction(nameof(Index));
                }
            }

            await _friendshipService.BlockUserAsync(userId, friendId);
            
            // Send real-time notification via SignalR
            var connectionId = CommunityCar.Infrastructure.Hubs.FriendHub.GetConnectionId(friendId);
            if (connectionId != null)
            {
                await _friendHubContext.Clients.Client(connectionId).SendAsync("UserBlocked", new
                {
                    BlockedBy = userId,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            TempData["Success"] = _localizer["UserBlocked"].Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking user {FriendId}", friendId);
            TempData["Error"] = _localizer["FailedToBlockUser"].Value;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("BlockUserJson")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BlockUserJson(Guid friendId)
    {
        try
        {
            // Validate friendId
            if (friendId == Guid.Empty)
            {
                return Json(new { success = false, message = _localizer["InvalidFriendId"].Value });
            }

            var userId = GetCurrentUserId();
            
            if (userId == friendId)
            {
                return Json(new { success = false, message = _localizer["CannotBlockSelf"].Value });
            }

            // Check if target user is an admin
            var targetUser = await _userManager.FindByIdAsync(friendId.ToString());
            if (targetUser != null)
            {
                var isAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");
                var isSuperAdmin = await _userManager.IsInRoleAsync(targetUser, "SuperAdmin");
                
                if (isAdmin || isSuperAdmin)
                {
                    return Json(new { success = false, message = _localizer["CannotBlockAdmin"].Value ?? "Cannot block administrators." });
                }
            }

            await _friendshipService.BlockUserAsync(userId, friendId);
            
            // Send real-time notification via SignalR
            var connectionId = CommunityCar.Infrastructure.Hubs.FriendHub.GetConnectionId(friendId);
            if (connectionId != null)
            {
                await _friendHubContext.Clients.Client(connectionId).SendAsync("UserBlocked", new
                {
                    BlockedBy = userId,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            return Json(new { success = true, message = _localizer["UserBlocked"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking user {FriendId}", friendId);
            return Json(new { success = false, message = _localizer["FailedToBlockUser"].Value });
        }
    }

    [HttpGet("Status/{friendId}")]
    public async Task<IActionResult> GetStatus(Guid friendId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var status = await _friendshipService.GetFriendshipStatusAsync(userId, friendId);
            return Json(new { success = true, status = status.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting friendship status for {FriendId}", friendId);
            return Json(new { success = false, message = _localizer["FailedToGetStatus"].Value });
        }
    }

    [HttpGet("Search")]
    public async Task<IActionResult> Search(string? query)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // If no query, show the search page
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new List<UserSearchViewModel>());
            }

            // Get all admin and superadmin user IDs to exclude them
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var superAdminUsers = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var adminUserIds = adminUsers.Select(u => u.Id).Concat(superAdminUsers.Select(u => u.Id)).ToList();

            var users = await _userManager.Users
                .Where(u => !u.IsDeleted && u.Id != userId &&
                    !adminUserIds.Contains(u.Id) && // Exclude admin users
                    ((u.FirstName != null && u.FirstName.Contains(query)) || 
                     (u.LastName != null && u.LastName.Contains(query)) || 
                     (u.UserName != null && u.UserName.Contains(query))))
                .Take(20)
                .ToListAsync();

            var viewModels = new List<UserSearchViewModel>();
            foreach (var user in users)
            {
                var status = await _friendshipService.GetFriendshipStatusAsync(userId, user.Id);
                viewModels.Add(new UserSearchViewModel
                {
                    UserId = user.Id,
                    Name = $"{user.FirstName} {user.LastName}",
                    UserName = user.UserName ?? string.Empty,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Slug = user.Slug ?? string.Empty,
                    FriendshipStatus = status
                });
            }

            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with query: {Query}", query);
            TempData["Error"] = _localizer["FailedToSearchUsers"].Value;
            return View(new List<UserSearchViewModel>());
        }
    }

    [HttpGet("SearchApi")]
    public async Task<IActionResult> SearchApi(string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { success = true, users = new List<object>() });
            }

            var userId = GetCurrentUserId();
            
            // Get all admin and superadmin user IDs to exclude them
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var superAdminUsers = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var adminUserIds = adminUsers.Select(u => u.Id).Concat(superAdminUsers.Select(u => u.Id)).ToList();
            
            var users = await _userManager.Users
                .Where(u => !u.IsDeleted && u.Id != userId &&
                    !adminUserIds.Contains(u.Id) && // Exclude admin users
                    ((u.FirstName != null && u.FirstName.Contains(query)) || 
                     (u.LastName != null && u.LastName.Contains(query)) || 
                     (u.UserName != null && u.UserName.Contains(query))))
                .Take(10)
                .Select(u => new
                {
                    id = u.Id,
                    name = $"{u.FirstName} {u.LastName}",
                    username = u.UserName,
                    profilePictureUrl = u.ProfilePictureUrl,
                    slug = u.Slug
                })
                .ToListAsync();

            return Json(new { success = true, users });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with query: {Query}", query);
            return Json(new { success = false, message = _localizer["FailedToSearchUsers"].Value });
        }
    }

    [HttpGet("Suggestions")]
    public async Task<IActionResult> Suggestions()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Get all admin and superadmin user IDs to exclude them
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var superAdminUsers = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var adminUserIds = adminUsers.Select(u => u.Id).Concat(superAdminUsers.Select(u => u.Id)).ToList();
            
            // Get users who are not friends and not blocked
            var suggestions = await _userManager.Users
                .Where(u => !u.IsDeleted && u.Id != userId && !adminUserIds.Contains(u.Id)) // Exclude admin users
                .Take(12)
                .ToListAsync();

            var viewModels = new List<UserSearchViewModel>();
            foreach (var user in suggestions)
            {
                var status = await _friendshipService.GetFriendshipStatusAsync(userId, user.Id);
                if (status == Domain.Enums.Community.friends.FriendshipStatus.None)
                {
                    viewModels.Add(new UserSearchViewModel
                    {
                        UserId = user.Id,
                        Name = $"{user.FirstName} {user.LastName}",
                        UserName = user.UserName ?? string.Empty,
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        Slug = user.Slug ?? string.Empty,
                        FriendshipStatus = status
                    });
                }
            }

            var friendships = await _friendshipService.GetFriendsAsync(userId);
            var requests = await _friendshipService.GetPendingRequestsAsync(userId);
            ViewData["FriendCount"] = friendships.Count();
            ViewData["RequestCount"] = requests.Count();
            return View(viewModels.Take(12).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading friend suggestions");
            TempData["Error"] = _localizer["FailedToLoadSuggestions"].Value;
            return View(new List<UserSearchViewModel>());
        }
    }

    [HttpGet("Blocked")]
    public async Task<IActionResult> Blocked()
    {
        try
        {
            var userId = GetCurrentUserId();
            var blocked = await _friendshipService.GetBlockedUsersAsync(userId);
            
            var viewModels = blocked.Select(b => new BlockedUserViewModel
            {
                UserId = b.FriendId,
                UserName = $"{b.Friend?.FirstName ?? "Unknown"} {b.Friend?.LastName ?? "User"}",
                ProfilePictureUrl = b.Friend?.ProfilePictureUrl,
                Slug = b.Friend?.Slug ?? string.Empty,
                BlockedAt = b.CreatedAt
            }).ToList();

            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading blocked users");
            TempData["Error"] = _localizer["FailedToLoadBlockedUsers"].Value;
            return View(new List<BlockedUserViewModel>());
        }
    }

    [HttpPost("UnblockUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnblockUser(Guid friendId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _friendshipService.UnblockUserAsync(userId, friendId);
            
            // Send real-time notification via SignalR
            var connectionId = CommunityCar.Infrastructure.Hubs.FriendHub.GetConnectionId(friendId);
            if (connectionId != null)
            {
                await _friendHubContext.Clients.Client(connectionId).SendAsync("UserUnblocked", new
                {
                    UnblockedBy = userId,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            TempData["Success"] = _localizer["UserUnblocked"].Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking user {FriendId}", friendId);
            TempData["Error"] = _localizer["FailedToUnblockUser"].Value;
        }

        return RedirectToAction(nameof(Blocked));
    }

    [HttpPost("UnblockUserJson")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnblockUserJson(Guid friendId)
    {
        try
        {
            // Validate friendId
            if (friendId == Guid.Empty)
            {
                return Json(new { success = false, message = _localizer["InvalidFriendId"].Value });
            }

            var userId = GetCurrentUserId();
            await _friendshipService.UnblockUserAsync(userId, friendId);
            
            // Send real-time notification via SignalR
            var connectionId = CommunityCar.Infrastructure.Hubs.FriendHub.GetConnectionId(friendId);
            if (connectionId != null)
            {
                await _friendHubContext.Clients.Client(connectionId).SendAsync("UserUnblocked", new
                {
                    UnblockedBy = userId,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            return Json(new { success = true, message = _localizer["UserUnblocked"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking user {FriendId}", friendId);
            return Json(new { success = false, message = _localizer["FailedToUnblockUser"].Value });
        }
    }
}
