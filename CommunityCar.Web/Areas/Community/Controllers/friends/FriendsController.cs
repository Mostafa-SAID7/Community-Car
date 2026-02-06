using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Web.Areas.Community.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Community.Controllers.friends;

[Area("Community")]
[Route("Community/[controller]")]
public class FriendsController : Controller
{
    private readonly IFriendshipService _friendshipService;
    private readonly UserManager<ApplicationUser> _userManager;
    public FriendsController(IFriendshipService friendshipService, UserManager<ApplicationUser> userManager)
    {
        _friendshipService = friendshipService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var friendships = await _friendshipService.GetFriendsAsync(userId);
        
        var viewModels = friendships.Select(f => new FriendshipViewModel
        {
            Id = f.Id,
            FriendId = f.FriendId,
            Slug = f.Friend.Slug,
            FriendName = $"{f.Friend.FirstName} {f.Friend.LastName}",
            ProfilePictureUrl = f.Friend.ProfilePictureUrl,
            Status = f.Status,
            Since = f.CreatedAt
        });

        return View(viewModels);
    }

    [HttpGet("Requests")]
    public async Task<IActionResult> Requests()
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        var requests = await _friendshipService.GetPendingRequestsAsync(userId);
        
        var viewModels = requests.Select(r => new FriendRequestViewModel
        {
            UserId = r.UserId,
            UserName = $"{r.User.FirstName} {r.User.LastName}",
            ProfilePictureUrl = r.User.ProfilePictureUrl,
            Slug = r.User.Slug,
            ReceivedAt = r.CreatedAt
        });

        return View(viewModels);
    }

    [HttpPost("SendRequest")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendRequest(Guid friendId)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        await _friendshipService.SendRequestAsync(userId, friendId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("AcceptRequest")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptRequest(Guid friendId)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);
        await _friendshipService.AcceptRequestAsync(userId, friendId);
        return RedirectToAction(nameof(Requests));
    }
}
