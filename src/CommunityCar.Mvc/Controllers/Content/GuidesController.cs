using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.guides;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.ViewModels.Guides;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Content;

[Route("Guides")]
public class GuidesController : Controller
{
    private readonly IGuideService _guideService;
    private readonly IFriendshipService _friendshipService;
    private readonly ILogger<GuidesController> _logger;

    public GuidesController(
        IGuideService guideService,
        IFriendshipService friendshipService,
        ILogger<GuidesController> logger)
    {
        _guideService = guideService;
        _friendshipService = friendshipService;
        _logger = logger;
    }

    // GET: Guides
    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 12,
        GuideDifficulty? difficulty = null,
        string? category = null)
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var currentUserId = GetCurrentUserId();

            var result = await _guideService.GetGuidesAsync(
                parameters,
                GuideStatus.Published,
                difficulty,
                category,
                currentUserId);

            ViewBag.CurrentDifficulty = difficulty;
            ViewBag.CurrentCategory = category;
            ViewBag.Difficulties = Enum.GetValues<GuideDifficulty>();
            ViewBag.Categories = await _guideService.GetCategoriesAsync();

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guides");
            TempData["Error"] = "Failed to load guides";
            return View(new PagedResult<Domain.DTOs.Community.GuideDto>(
                new List<Domain.DTOs.Community.GuideDto>(), 0, page, pageSize));
        }
    }

    // GET: Guides/Details/{slug}
    [HttpGet("Details/{slug}")]
    [HttpGet("Details")]
    public async Task<IActionResult> Details(string slug)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var guideDto = await _guideService.GetGuideBySlugAsync(slug, currentUserId);

            if (guideDto == null)
            {
                TempData["Error"] = "Guide not found";
                return RedirectToAction(nameof(Index));
            }

            // Increment view count
            await _guideService.IncrementViewCountAsync(guideDto.Id);

            if (currentUserId.HasValue)
            {
                var status = await _friendshipService.GetFriendshipStatusAsync(currentUserId.Value, guideDto.AuthorId);
                ViewBag.FriendshipStatus = status;
                
                if (status == CommunityCar.Domain.Enums.Community.friends.FriendshipStatus.Pending)
                {
                    var pendingRequests = await _friendshipService.GetPendingRequestsAsync(currentUserId.Value);
                    ViewBag.IsIncomingRequest = pendingRequests.Any(r => r.UserId == guideDto.AuthorId);
                }
            }
            else
            {
                ViewBag.FriendshipStatus = CommunityCar.Domain.Enums.Community.friends.FriendshipStatus.None;
            }

            return View(guideDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guide {Slug}", slug);
            TempData["Error"] = "Failed to load guide";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Guides/Create
    [Authorize]
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Difficulties = Enum.GetValues<GuideDifficulty>();
        ViewBag.Categories = await _guideService.GetCategoriesAsync();
        return View(new CreateGuideViewModel());
    }

    // POST: Guides/Create
    [Authorize]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGuideViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Difficulties = Enum.GetValues<GuideDifficulty>();
                ViewBag.Categories = await _guideService.GetCategoriesAsync();
                return View(model);
            }

            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();

            var guide = await _guideService.CreateGuideAsync(
                model.Title,
                model.Content,
                model.Summary,
                model.Category,
                userId,
                model.Difficulty,
                model.EstimatedTimeMinutes);

            TempData["Success"] = "Guide created successfully";
            return RedirectToAction(nameof(Details), new { slug = guide.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating guide");
            ModelState.AddModelError("", "Failed to create guide");
            ViewBag.Difficulties = Enum.GetValues<GuideDifficulty>();
            ViewBag.Categories = await _guideService.GetCategoriesAsync();
            return View(model);
        }
    }

    // GET: Guides/Edit/{id}
    [Authorize]
    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            var guideDto = await _guideService.GetGuideByIdAsync(id, userId);

            if (guideDto == null)
            {
                TempData["Error"] = "Guide not found";
                return RedirectToAction(nameof(Index));
            }

            if (!guideDto.IsAuthor)
            {
                TempData["Error"] = "You can only edit your own guides";
                return RedirectToAction(nameof(Details), new { slug = guideDto.Slug });
            }

            var model = new EditGuideViewModel
            {
                Id = guideDto.Id,
                Title = guideDto.Title,
                Content = guideDto.Content,
                Summary = guideDto.Summary,
                Category = guideDto.Category,
                Difficulty = guideDto.Difficulty,
                EstimatedTimeMinutes = guideDto.EstimatedTimeMinutes,
                Tags = guideDto.Tags,
                ImageUrl = guideDto.ImageUrl,
                VideoUrl = guideDto.VideoUrl
            };

            ViewBag.Difficulties = Enum.GetValues<GuideDifficulty>();
            ViewBag.Categories = await _guideService.GetCategoriesAsync();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guide for edit {GuideId}", id);
            TempData["Error"] = "Failed to load guide";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Guides/Edit/{id}
    [Authorize]
    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditGuideViewModel model)
    {
        try
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Difficulties = Enum.GetValues<GuideDifficulty>();
                ViewBag.Categories = await _guideService.GetCategoriesAsync();
                return View(model);
            }

            var guide = await _guideService.UpdateGuideAsync(
                id,
                model.Title,
                model.Content,
                model.Summary,
                model.Category,
                model.Difficulty,
                model.EstimatedTimeMinutes);

            TempData["Success"] = "Guide updated successfully";
            return RedirectToAction(nameof(Details), new { slug = guide.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating guide {GuideId}", id);
            ModelState.AddModelError("", "Failed to update guide");
            ViewBag.Difficulties = Enum.GetValues<GuideDifficulty>();
            ViewBag.Categories = await _guideService.GetCategoriesAsync();
            return View(model);
        }
    }

    // POST: Guides/Delete/{id}
    [Authorize]
    [HttpPost("Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _guideService.DeleteGuideAsync(id);
            return Json(new { success = true, message = "Guide deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting guide {GuideId}", id);
            return Json(new { success = false, message = "Failed to delete guide" });
        }
    }

    // POST: Guides/Publish/{id}
    [Authorize]
    [HttpPost("Publish/{id:guid}")]
    public async Task<IActionResult> Publish(Guid id)
    {
        try
        {
            await _guideService.PublishGuideAsync(id);
            return Json(new { success = true, message = "Guide published successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing guide {GuideId}", id);
            return Json(new { success = false, message = "Failed to publish guide" });
        }
    }

    // POST: Guides/Archive/{id}
    [Authorize]
    [HttpPost("Archive/{id:guid}")]
    public async Task<IActionResult> Archive(Guid id)
    {
        try
        {
            await _guideService.ArchiveGuideAsync(id);
            return Json(new { success = true, message = "Guide archived successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving guide {GuideId}", id);
            return Json(new { success = false, message = "Failed to archive guide" });
        }
    }

    // POST: Guides/ToggleLike/{id}
    [Authorize]
    [HttpPost("ToggleLike/{id:guid}")]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _guideService.ToggleLikeAsync(id, userId);

            return Json(new { success = true, message = "Like toggled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling like for guide {GuideId}", id);
            return Json(new { success = false, message = "Failed to toggle like" });
        }
    }

    // POST: Guides/ToggleBookmark/{id}
    [Authorize]
    [HttpPost("ToggleBookmark/{id:guid}")]
    public async Task<IActionResult> ToggleBookmark(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _guideService.ToggleBookmarkAsync(id, userId);

            return Json(new { success = true, message = "Bookmark toggled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling bookmark for guide {GuideId}", id);
            return Json(new { success = false, message = "Failed to toggle bookmark" });
        }
    }

    // GET: Guides/MyGuides
    [Authorize]
    [HttpGet("MyGuides")]
    public async Task<IActionResult> MyGuides(int page = 1, int pageSize = 12)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var result = await _guideService.GetUserGuidesAsync(userId, parameters, userId);

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user guides");
            TempData["Error"] = "Failed to load your guides";
            return RedirectToAction(nameof(Index));
        }
    }

    private Guid? GetCurrentUserId()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return null;

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
