using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.maps;
using CommunityCar.Mvc.ViewModels.Maps;
using CommunityCar.Mvc.Controllers.Base;

namespace CommunityCar.Mvc.Controllers.Community;

public class MapsController : BaseController
{
    private readonly IMapService _mapService;
    private readonly IMapper _mapper;
    private readonly ILogger<MapsController> _logger;

    public MapsController(
        IMapService mapService,
        IMapper mapper,
        ILogger<MapsController> logger)
    {
        _mapService = mapService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(
        int page = 1,
        MapPointType? type = null,
        string? search = null)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var parameters = new QueryParameters { PageNumber = page, PageSize = 20 };

            var result = string.IsNullOrEmpty(search)
                ? await _mapService.GetMapPointsAsync(parameters, MapPointStatus.Published, type, null, userId)
                : await _mapService.SearchMapPointsAsync(search, parameters, userId);

            ViewBag.Type = type;
            ViewBag.SearchTerm = search;

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading map points");
            return View(new PagedResult<Domain.DTOs.Community.MapPointDto>());
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Map(MapPointType? type = null)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var parameters = new QueryParameters { PageNumber = 1, PageSize = 1000 };
            var result = await _mapService.GetMapPointsAsync(parameters, MapPointStatus.Published, type, null, userId);

            ViewBag.Type = type;
            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading map view");
            return View(new PagedResult<Domain.DTOs.Community.MapPointDto>());
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(string slug)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var mapPoint = await _mapService.GetMapPointBySlugAsync(slug, userId);

            if (mapPoint == null)
                return NotFound();

            await _mapService.IncrementViewsAsync(mapPoint.Id);

            return View(mapPoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading map point details");
            return NotFound();
        }
    }

    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        ViewBag.Types = Enum.GetValues(typeof(MapPointType));
        return View(new CreateMapPointViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMapPointViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Types = Enum.GetValues(typeof(MapPointType));
            return View(model);
        }

        try
        {
            var userId = GetCurrentUserId();
            var mapPoint = await _mapService.CreateMapPointAsync(
                model.Name,
                model.Latitude,
                model.Longitude,
                model.Address,
                model.Type,
                userId,
                model.Description);

            TempData["Success"] = "Map point created successfully!";
            return RedirectToAction(nameof(Details), new { slug = mapPoint.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating map point");
            ModelState.AddModelError("", "An error occurred while creating the map point.");
            ViewBag.Types = Enum.GetValues(typeof(MapPointType));
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
            var mapPoint = await _mapService.GetMapPointByIdAsync(id, userId);

            if (mapPoint == null)
                return NotFound();

            if (mapPoint.OwnerId != userId)
                return Forbid();

            var viewModel = _mapper.Map<EditMapPointViewModel>(mapPoint);
            ViewBag.Types = Enum.GetValues(typeof(MapPointType));
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading map point for edit");
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditMapPointViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Types = Enum.GetValues(typeof(MapPointType));
            return View(model);
        }

        try
        {
            var userId = GetCurrentUserId();
            var mapPoint = await _mapService.GetMapPointByIdAsync(id, userId);

            if (mapPoint == null)
                return NotFound();

            if (mapPoint.OwnerId != userId)
                return Forbid();

            var updatedMapPoint = await _mapService.UpdateMapPointAsync(
                id,
                model.Name,
                model.Latitude,
                model.Longitude,
                model.Address,
                model.Type,
                model.Description);

            TempData["Success"] = "Map point updated successfully!";
            return RedirectToAction(nameof(Details), new { slug = updatedMapPoint.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating map point");
            ModelState.AddModelError("", "An error occurred while updating the map point.");
            ViewBag.Types = Enum.GetValues(typeof(MapPointType));
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
            var mapPoint = await _mapService.GetMapPointByIdAsync(id, userId);

            if (mapPoint == null)
                return NotFound();

            if (mapPoint.OwnerId != userId)
                return Forbid();

            await _mapService.DeleteMapPointAsync(id);

            TempData["Success"] = "Map point deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting map point");
            TempData["Error"] = "An error occurred while deleting the map point.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Nearby(double latitude, double longitude, double radius = 10, MapPointType? type = null)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var mapPoints = await _mapService.GetNearbyMapPointsAsync(latitude, longitude, radius, type, userId);

            return Json(new { success = true, mapPoints });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting nearby map points");
            return Json(new { success = false, message = "Failed to get nearby locations" });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Featured()
    {
        try
        {
            var mapPoints = await _mapService.GetFeaturedMapPointsAsync(20);
            return View(mapPoints);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading featured map points");
            return View(new List<Domain.DTOs.Community.MapPointDto>());
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleFavorite(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _mapService.ToggleFavoriteAsync(id, userId);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling favorite");
            return Json(new { success = false, message = "Failed to toggle favorite" });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _mapService.CheckInAsync(id, userId);
            return Json(new { success = true, message = "Checked in successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in");
            return Json(new { success = false, message = "Failed to check in" });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddRating(Guid id, int rating, string? comment = null)
    {
        if (rating < 1 || rating > 5)
        {
            return Json(new { success = false, message = "Rating must be between 1 and 5" });
        }

        try
        {
            var userId = GetCurrentUserId();
            await _mapService.AddOrUpdateRatingAsync(id, userId, rating, comment);
            return Json(new { success = true, message = "Rating added successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding rating");
            return Json(new { success = false, message = "Failed to add rating" });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteRating(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _mapService.DeleteRatingAsync(id, userId);
            return Json(new { success = true, message = "Rating deleted successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting rating");
            return Json(new { success = false, message = "Failed to delete rating" });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddComment(Guid mapPointId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Json(new { success = false, message = "Comment content is required" });
        }

        try
        {
            var userId = GetCurrentUserId();
            var comment = await _mapService.AddCommentAsync(mapPointId, userId, content);
            return Json(new { success = true, comment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment");
            return Json(new { success = false, message = "Failed to add comment" });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateComment(Guid commentId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Json(new { success = false, message = "Comment content is required" });
        }

        try
        {
            var userId = GetCurrentUserId();
            var comment = await _mapService.UpdateCommentAsync(commentId, userId, content);
            return Json(new { success = true, comment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating comment");
            return Json(new { success = false, message = "Failed to update comment" });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _mapService.DeleteCommentAsync(commentId, userId);
            return Json(new { success = true, message = "Comment deleted successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment");
            return Json(new { success = false, message = "Failed to delete comment" });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetComments(Guid mapPointId, int page = 1)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var parameters = new QueryParameters { PageNumber = page, PageSize = 20 };
            var result = await _mapService.GetMapPointCommentsAsync(mapPointId, parameters, userId);

            return PartialView("_CommentsList", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading comments");
            return PartialView("_CommentsList", new PagedResult<Domain.DTOs.Community.MapPointCommentDto>());
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MyMapPoints(int page = 1)
    {
        try
        {
            var userId = GetCurrentUserId();
            var parameters = new QueryParameters { PageNumber = page, PageSize = 20 };
            var result = await _mapService.GetMapPointsAsync(parameters, null, null, userId, userId);

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user map points");
            return View(new PagedResult<Domain.DTOs.Community.MapPointDto>());
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetMapData(MapPointType? type = null)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var parameters = new QueryParameters { PageNumber = 1, PageSize = 1000 };
            var result = await _mapService.GetMapPointsAsync(parameters, MapPointStatus.Published, type, null, userId);

            var mapData = result.Items.Select(mp => new
            {
                id = mp.Id,
                name = mp.Name,
                latitude = mp.Latitude,
                longitude = mp.Longitude,
                type = mp.Type.ToString(),
                address = mp.Address,
                description = mp.Description,
                averageRating = mp.AverageRating,
                isVerified = mp.IsVerified,
                slug = mp.Slug
            });

            return Json(new { success = true, data = mapData });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting map data");
            return Json(new { success = false, message = "Failed to load map data" });
        }
    }
}
