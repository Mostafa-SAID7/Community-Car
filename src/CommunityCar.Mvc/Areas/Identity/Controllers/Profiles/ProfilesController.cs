using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.friends;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Web.Areas.Identity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Mvc.Areas.Identity.Controllers.Profiles;

[Area("Identity")]
[Route("{culture}/Identity/[controller]")]
public class ProfilesController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IFriendshipService _friendshipService;
    private readonly IQuestionService _questionService;
    private readonly ILogger<ProfilesController> _logger;
    private readonly ICurrentUserService _currentUserService;

    public ProfilesController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IFriendshipService friendshipService,
        IQuestionService questionService,
        ILogger<ProfilesController> logger,
        ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _friendshipService = friendshipService;
        _questionService = questionService;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    #region Profile View

    [HttpGet]
    [Route("{id?}")]
    [Route("Index/{id?}")]
    public async Task<IActionResult> Index(string? id)
    {
        var currentUserId = _currentUserService.UserId;
        ApplicationUser? user = null;

        if (string.IsNullOrEmpty(id))
        {
            if (currentUserId == null || currentUserId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            user = await _userManager.FindByIdAsync(currentUserId.ToString()!);
        }
        else if (Guid.TryParse(id, out var userId))
        {
            user = await _userManager.FindByIdAsync(userId.ToString());
        }
        else
        {
            user = await _userManager.Users.FirstOrDefaultAsync(u => u.Slug == id);
        }

        if (user == null)
        {
            return NotFound("User not found.");
        }

        var targetUserId = user.Id;
        var isOwnProfile = currentUserId == targetUserId;
        var friendshipStatus = FriendshipStatus.None;
        
        if (!isOwnProfile && currentUserId != null)
        {
            friendshipStatus = await _friendshipService.GetFriendshipStatusAsync(currentUserId.Value, targetUserId);
        }

        // Get user statistics
        var userQuestions = await _questionService.GetUserQuestionsAsync(targetUserId, new CommunityCar.Domain.Base.QueryParameters { PageSize = 100 });
        var friends = await _friendshipService.GetFriendsAsync(targetUserId);
        var friendsCount = friends.Count();

        var viewModel = new ProfileViewModel
        {
            User = user,
            IsOwnProfile = isOwnProfile,
            FriendshipStatus = friendshipStatus,
            QuestionsCount = userQuestions.TotalCount,
            FriendsCount = friendsCount,
            RecentQuestions = userQuestions.Items.Take(5).ToList(),
            JoinedDate = user.CreatedAt.DateTime
        };

        return View(viewModel);
    }

    #endregion

    #region Profile Edit

    [HttpGet]
    [Authorize]
    [Route("Edit")]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var model = new EditProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Bio = user.Bio,
            Location = user.Location ?? string.Empty,
            Website = user.Website ?? string.Empty,
            DateOfBirth = user.DateOfBirth
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [Route("Edit")]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Bio = model.Bio;
        user.Location = model.Location;
        user.Website = model.Website;
        user.DateOfBirth = model.DateOfBirth;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserId} updated their profile", user.Id);
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    #endregion

    #region Profile Picture

    [HttpGet]
    [Authorize]
    [Route("UploadPicture")]
    public IActionResult UploadPicture()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [Route("UploadPicture")]
    public async Task<IActionResult> UploadPicture(IFormFile? profilePicture)
    {
        if (profilePicture == null || profilePicture.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Please select a file.");
            return View();
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(profilePicture.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError(string.Empty, "Invalid file type. Only JPG, PNG, and GIF are allowed.");
            return View();
        }

        // Validate file size (max 5MB)
        if (profilePicture.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError(string.Empty, "File size must be less than 5MB.");
            return View();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        try
        {
            // Save file to wwwroot/uploads/profiles
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{user.Id}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(stream);
            }

            // Update user profile picture URL
            user.ProfilePictureUrl = $"/uploads/profiles/{uniqueFileName}";
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User {UserId} uploaded profile picture", user.Id);
            TempData["Success"] = "Profile picture updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile picture for user {UserId}", user.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while uploading the file.");
            return View();
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [Route("RemovePicture")]
    public async Task<IActionResult> RemovePicture()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
        {
            // Delete physical file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            user.ProfilePictureUrl = null;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User {UserId} removed profile picture", user.Id);
            TempData["Success"] = "Profile picture removed successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Activity & Statistics

    [HttpGet]
    [Route("Activity/{userId?}")]
    public async Task<IActionResult> Activity(Guid? userId)
    {
        var targetUserId = userId ?? _currentUserService.UserId;

        if (targetUserId == null || targetUserId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var user = await _userManager.FindByIdAsync(targetUserId.ToString()!);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var questions = await _questionService.GetUserQuestionsAsync(targetUserId.Value, new CommunityCar.Domain.Base.QueryParameters { PageSize = 100 });
        
        var viewModel = new UserActivityViewModel
        {
            User = user,
            RecentQuestions = questions.Items.OrderByDescending(q => q.CreatedAt).Take(10).ToList(),
            TotalQuestions = questions.TotalCount,
            TotalAnswers = 0, // TODO: Implement answer count
            TotalVotes = 0 // TODO: Implement vote count
        };

        return View(viewModel);
    }

    [HttpGet]
    [Route("Statistics/{userId?}")]
    public async Task<IActionResult> Statistics(Guid? userId)
    {
        var targetUserId = userId ?? _currentUserService.UserId;

        if (targetUserId == null || targetUserId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var user = await _userManager.FindByIdAsync(targetUserId.ToString()!);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var questions = await _questionService.GetUserQuestionsAsync(targetUserId.Value, new CommunityCar.Domain.Base.QueryParameters { PageSize = 100 });
        var friends = await _friendshipService.GetFriendsAsync(targetUserId.Value);
        var friendsCount = friends.Count();

        var viewModel = new UserStatisticsViewModel
        {
            User = user,
            TotalQuestions = questions.TotalCount,
            TotalAnswers = 0, // TODO: Implement
            TotalVotes = 0, // TODO: Implement
            TotalFriends = friendsCount,
            ReputationPoints = user.ReputationPoints,
            BadgesCount = 0, // TODO: Implement
            MemberSince = user.CreatedAt.DateTime
        };

        return View(viewModel);
    }

    #endregion

    #region Friends & Social

    [HttpGet]
    [Route("Friends/{userId?}")]
    public async Task<IActionResult> Friends(Guid? userId)
    {
        var targetUserId = userId ?? _currentUserService.UserId;

        if (targetUserId == null || targetUserId == Guid.Empty)
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var user = await _userManager.FindByIdAsync(targetUserId.ToString()!);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var friends = await _friendshipService.GetFriendsAsync(targetUserId.Value);
        
        // Map Friendship entities to FriendshipDto
        var friendDtos = friends.Select(f => new CommunityCar.Domain.DTOs.Community.FriendshipDto
        {
            Id = f.Id,
            FriendId = f.UserId == targetUserId.Value ? f.FriendId : f.UserId,
            FriendName = f.UserId == targetUserId.Value ? f.Friend.FullName ?? "Unknown" : f.User.FullName ?? "Unknown",
            ProfilePictureUrl = f.UserId == targetUserId.Value ? f.Friend.ProfilePictureUrl : f.User.ProfilePictureUrl,
            Slug = f.UserId == targetUserId.Value ? f.Friend.Slug : f.User.Slug,
            Status = f.Status,
            Since = f.CreatedAt
        }).ToList();

        var viewModel = new UserFriendsViewModel
        {
            User = user,
            Friends = friendDtos,
            IsOwnProfile = _currentUserService.UserId == targetUserId
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [Route("SendFriendRequest/{userId}")]
    public async Task<IActionResult> SendFriendRequest(Guid userId)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        try
        {
            await _friendshipService.SendRequestAsync(currentUserId.Value, userId);
            _logger.LogInformation("User {CurrentUserId} sent friend request to {TargetUserId}", currentUserId, userId);
            TempData["Success"] = "Friend request sent successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending friend request");
            TempData["Error"] = "Failed to send friend request.";
        }

        return RedirectToAction(nameof(Index), new { userId });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [Route("AcceptFriendRequest/{userId}")]
    public async Task<IActionResult> AcceptFriendRequest(Guid userId)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        try
        {
            await _friendshipService.AcceptRequestAsync(userId, currentUserId.Value);
            _logger.LogInformation("User {CurrentUserId} accepted friend request from {TargetUserId}", currentUserId, userId);
            TempData["Success"] = "Friend request accepted.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting friend request");
            TempData["Error"] = "Failed to accept friend request.";
        }

        return RedirectToAction(nameof(Index), new { userId });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [Route("RemoveFriend/{userId}")]
    public async Task<IActionResult> RemoveFriend(Guid userId)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null)
        {
            return Unauthorized();
        }

        try
        {
            await _friendshipService.RemoveFriendAsync(currentUserId.Value, userId);
            _logger.LogInformation("User {CurrentUserId} removed friend {TargetUserId}", currentUserId, userId);
            TempData["Success"] = "Friend removed successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing friend");
            TempData["Error"] = "Failed to remove friend.";
        }

        return RedirectToAction(nameof(Index), new { userId });
    }

    #endregion

    #region Settings

    [HttpGet]
    [Authorize]
    [Route("Settings")]
    public async Task<IActionResult> Settings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var model = new ProfileSettingsViewModel
        {
            EmailNotifications = true, // TODO: Get from user preferences
            SmsNotifications = false,
            PublicProfile = true,
            ShowEmail = false,
            ShowPhone = false
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [Route("Settings")]
    public async Task<IActionResult> Settings(ProfileSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // TODO: Save user preferences to database

        _logger.LogInformation("User {UserId} updated profile settings", user.Id);
        TempData["Success"] = "Settings updated successfully.";
        return RedirectToAction(nameof(Settings));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [Route("DeleteAccount")]
    public async Task<IActionResult> DeleteAccount(string password)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Verify password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            TempData["Error"] = "Invalid password.";
            return RedirectToAction(nameof(Settings));
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User {UserId} deleted their account", user.Id);
            return RedirectToAction("Index", "Feed", new { area = "" });
        }

        TempData["Error"] = "Failed to delete account.";
        return RedirectToAction(nameof(Settings));
    }

    #endregion

    #region Search & Discovery

    [HttpGet]
    [Route("Search")]
    public async Task<IActionResult> Search(string? query, int page = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return View(new UserSearchResultsViewModel { Query = string.Empty, Users = new List<ApplicationUser>() });
        }

        var users = await _userManager.Users
            .Where(u => u.FirstName.Contains(query) || 
                       u.LastName.Contains(query) || 
                       u.Email!.Contains(query))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var viewModel = new UserSearchResultsViewModel
        {
            Query = query,
            Users = users,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = await _userManager.Users.CountAsync(u => 
                u.FirstName.Contains(query) || 
                u.LastName.Contains(query) || 
                u.Email!.Contains(query))
        };

        return View(viewModel);
    }

    #endregion
}
