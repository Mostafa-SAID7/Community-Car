using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Web.ViewModels.Community;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CommunityCar.Web.Controllers.Social;

[Route("[controller]")]
[Authorize]
public class FriendsController : Controller
{
    private readonly IFriendshipService _friendshipService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<FriendsController> _logger;

    public FriendsController(
        IFriendshipService friendshipService, 
        UserManager<ApplicationUser> userManager,
        ILogger<FriendsController> logger)
    {
        _friendshipService = friendshipService;
        _userManager = userManager;
        _logger = logger;
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
            
            var viewModels = friendships.Select(f => new FriendshipViewModel
            {
                Id = f.Id,
                FriendId = f.FriendId,
                Slug = f.Friend?.Slug ?? string.Empty,
                FriendName = $"{f.Friend?.FirstName ?? "Unknown"} {f.Friend?.LastName ?? "User"}",
                ProfilePictureUrl = f.Friend?.ProfilePictureUrl,
                Status = f.Status,
                Since = f.CreatedAt
            }).ToList();

            ViewBag.FriendCount = viewModels.Count;
            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading friends list");
            TempData["Error"] = "Failed to load friends list. Please try again.";
            return View(new List<FriendshipViewModel>());
        }
    }

    [HttpGet("Requests")]
    public async Task<IActionResult> Requests()
    {
        try
        {
            var userId = GetCurrentUserId();
            var requests = await _friendshipService.GetPendingRequestsAsync(userId);
            
            var viewModels = requests.Select(r => new FriendRequestViewModel
            {
                UserId = r.UserId,
                UserName = $"{r.User?.FirstName ?? "Unknown"} {r.User?.LastName ?? "User"}",
                ProfilePictureUrl = r.User?.ProfilePictureUrl,
                Slug = r.User?.Slug ?? string.Empty,
                ReceivedAt = r.CreatedAt
            }).ToList();

            ViewBag.RequestCount = viewModels.Count;
            return View(viewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading friend requests");
            TempData["Error"] = "Failed to load friend requests. Please try again.";
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
                TempData["Error"] = "You cannot send a friend request to yourself.";
                return RedirectToAction(nameof(Index));
            }

            await _friendshipService.SendRequestAsync(userId, friendId);
            TempData["Success"] = "Friend request sent successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend request to {FriendId}", friendId);
            TempData["Error"] = "Failed to send friend request. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("AcceptRequest")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptRequest(Guid friendId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _friendshipService.AcceptRequestAsync(userId, friendId);
            TempData["Success"] = "Friend request accepted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting friend request from {FriendId}", friendId);
            TempData["Error"] = "Failed to accept friend request. Please try again.";
        }

        return RedirectToAction(nameof(Requests));
    }

    [HttpPost("RejectRequest")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectRequest(Guid friendId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _friendshipService.RejectRequestAsync(userId, friendId);
            TempData["Success"] = "Friend request rejected.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting friend request from {FriendId}", friendId);
            TempData["Error"] = "Failed to reject friend request. Please try again.";
        }

        return RedirectToAction(nameof(Requests));
    }

    [HttpPost("RemoveFriend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFriend(Guid friendId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _friendshipService.RemoveFriendAsync(userId, friendId);
            TempData["Success"] = "Friend removed successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing friend {FriendId}", friendId);
            TempData["Error"] = "Failed to remove friend. Please try again.";
        }

        return RedirectToAction(nameof(Index));
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
                TempData["Error"] = "You cannot block yourself.";
                return RedirectToAction(nameof(Index));
            }

            await _friendshipService.BlockUserAsync(userId, friendId);
            TempData["Success"] = "User blocked successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking user {FriendId}", friendId);
            TempData["Error"] = "Failed to block user. Please try again.";
        }

        return RedirectToAction(nameof(Index));
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
            return Json(new { success = false, message = "Failed to get friendship status" });
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

            var users = await _userManager.Users
                .Where(u => !u.IsDeleted && u.Id != userId &&
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
            TempData["Error"] = "Failed to search users. Please try again.";
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
            var users = await _userManager.Users
                .Where(u => !u.IsDeleted && u.Id != userId &&
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
            return Json(new { success = false, message = "Failed to search users" });
        }
    }

    [HttpGet("Suggestions")]
    public async Task<IActionResult> Suggestions()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Get users who are not friends and not blocked
            var suggestions = await _userManager.Users
                .Where(u => !u.IsDeleted && u.Id != userId)
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

            return View(viewModels.Take(12).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading friend suggestions");
            TempData["Error"] = "Failed to load suggestions. Please try again.";
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
            TempData["Error"] = "Failed to load blocked users. Please try again.";
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
            TempData["Success"] = "User unblocked successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking user {FriendId}", friendId);
            TempData["Error"] = "Failed to unblock user. Please try again.";
        }

        return RedirectToAction(nameof(Blocked));
    }
}
