using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.guides;
using CommunityCar.Mvc.ViewModels.Guides;
using CommunityCar.Mvc.Controllers.Base;

namespace CommunityCar.Mvc.Controllers.Community;

public class GuidesController : BaseController
{
    private readonly IGuideService _guideService;
    private readonly IMapper _mapper;
    private readonly ILogger<GuidesController> _logger;

    public GuidesController(
        IGuideService guideService,
        IMapper mapper,
        ILogger<GuidesController> logger)
    {
        _guideService = guideService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(
        int page = 1,
        string? search = null,
        string? category = null,
        GuideDifficulty? difficulty = null)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var parameters = new QueryParameters { PageNumber = page, PageSize = 12 };

            var result = string.IsNullOrEmpty(search)
                ? await _guideService.GetGuidesAsync(parameters, GuideStatus.Published, difficulty, category, userId)
                : await _guideService.SearchGuidesAsync(search, parameters, userId);

            var categories = await _guideService.GetCategoriesAsync();

            ViewBag.SearchTerm = search;
            ViewBag.Category = category;
            ViewBag.Difficulty = difficulty;
            ViewBag.Categories = categories;

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guides");
            return View(new PagedResult<Domain.DTOs.Community.GuideDto>());
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MyGuides(int page = 1)
    {
        try
        {
            var userId = GetCurrentUserId();
            var parameters = new QueryParameters { PageNumber = page, PageSize = 12 };
            var result = await _guideService.GetUserGuidesAsync(userId, parameters, userId);

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user guides");
            return View(new PagedResult<Domain.DTOs.Community.GuideDto>());
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(string slug)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var guide = await _guideService.GetGuideBySlugAsync(slug, userId);

            if (guide == null)
                return NotFound();

            await _guideService.IncrementViewCountAsync(guide.Id);

            return View(guide);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guide details");
            return NotFound();
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Create()
    {
        var categories = await _guideService.GetCategoriesAsync();
        ViewBag.Categories = categories;
        return View(new CreateGuideViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGuideViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _guideService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            return View(model);
        }

        try
        {
            var userId = GetCurrentUserId();
            var guide = await _guideService.CreateGuideAsync(
                model.Title,
                model.Content,
                model.Summary,
                model.Category,
                userId,
                model.Difficulty,
                model.EstimatedTimeMinutes);

            TempData["Success"] = "Guide created successfully!";
            return RedirectToAction(nameof(Details), new { slug = guide.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating guide");
            ModelState.AddModelError("", "An error occurred while creating the guide.");
            var categories = await _guideService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            return View(model);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var guide = await _guideService.GetGuideByIdAsync(id, userId);

            if (guide == null)
                return NotFound();

            if (guide.AuthorId != userId)
                return Forbid();

            var categories = await _guideService.GetCategoriesAsync();
            var viewModel = _mapper.Map<EditGuideViewModel>(guide);
            ViewBag.Categories = categories;

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guide for edit");
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditGuideViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            var categories = await _guideService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            return View(model);
        }

        try
        {
            var userId = GetCurrentUserId();
            var guide = await _guideService.GetGuideByIdAsync(id, userId);

            if (guide == null)
                return NotFound();

            if (guide.AuthorId != userId)
                return Forbid();

            var updatedGuide = await _guideService.UpdateGuideAsync(
                id,
                model.Title,
                model.Content,
                model.Summary,
                model.Category,
                model.Difficulty,
                model.EstimatedTimeMinutes);

            TempData["Success"] = "Guide updated successfully!";
            return RedirectToAction(nameof(Details), new { slug = updatedGuide.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating guide");
            ModelState.AddModelError("", "An error occurred while updating the guide.");
            var categories = await _guideService.GetCategoriesAsync();
            ViewBag.Categories = categories;
            return View(model);
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var guide = await _guideService.GetGuideByIdAsync(id, userId);

            if (guide == null)
                return NotFound();

            if (guide.AuthorId != userId)
                return Forbid();

            await _guideService.DeleteGuideAsync(id);

            TempData["Success"] = "Guide deleted successfully!";
            return RedirectToAction(nameof(MyGuides));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting guide");
            TempData["Error"] = "An error occurred while deleting the guide.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Publish(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var guide = await _guideService.GetGuideByIdAsync(id, userId);

            if (guide == null)
                return Json(new { success = false, message = "Guide not found" });

            if (guide.AuthorId != userId)
                return Json(new { success = false, message = "Unauthorized" });

            await _guideService.PublishGuideAsync(id);
            return Json(new { success = true, message = "Guide published successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing guide");
            return Json(new { success = false, message = "Failed to publish guide" });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Archive(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var guide = await _guideService.GetGuideByIdAsync(id, userId);

            if (guide == null)
                return Json(new { success = false, message = "Guide not found" });

            if (guide.AuthorId != userId)
                return Json(new { success = false, message = "Unauthorized" });

            await _guideService.ArchiveGuideAsync(id);
            return Json(new { success = true, message = "Guide archived successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving guide");
            return Json(new { success = false, message = "Failed to archive guide" });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _guideService.ToggleLikeAsync(id, userId);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling like");
            return Json(new { success = false, message = "Failed to toggle like" });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleBookmark(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _guideService.ToggleBookmarkAsync(id, userId);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling bookmark");
            return Json(new { success = false, message = "Failed to toggle bookmark" });
        }
    }
}
