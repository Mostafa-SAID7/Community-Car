using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.maps;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.ViewModels.Maps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Location;

[Route("{culture:alpha}/Maps")]
public class MapsController : Controller
{
    private readonly IMapService _mapService;
    private readonly ILogger<MapsController> _logger;
    private readonly IStringLocalizer<MapsController> _localizer;

    public MapsController(
        IMapService mapService,
        ILogger<MapsController> logger,
        IStringLocalizer<MapsController> localizer)
    {
        _mapService = mapService;
        _logger = logger;
        _localizer = localizer;
    }

    // GET: Maps
    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 12,
        MapPointType? type = null,
        MapPointStatus? status = null)
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var currentUserId = GetCurrentUserId();

            var result = await _mapService.GetMapPointsAsync(
                parameters,
                status ?? MapPointStatus.Published,
                type,
                null,
                currentUserId);

            ViewBag.CurrentType = type;
            ViewBag.CurrentStatus = status;
            ViewBag.Types = Enum.GetValues<MapPointType>();

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading map points");
            TempData["Error"] = _localizer["FailedToLoadMapPoints"].Value;
            return View(new PagedResult<Domain.DTOs.Community.MapPointDto>(
                new List<Domain.DTOs.Community.MapPointDto>(), 0, page, pageSize));
        }
    }

    // GET: Maps/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var mapPointDto = await _mapService.GetMapPointBySlugAsync(slug, currentUserId);

            if (mapPointDto == null)
            {
                TempData["Error"] = _localizer["MapPointNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            await _mapService.IncrementViewsAsync(mapPointDto.Id);

            var commentsParams = new QueryParameters { PageNumber = 1, PageSize = 10 };
            var comments = await _mapService.GetMapPointCommentsAsync(
                mapPointDto.Id,
                commentsParams,
                currentUserId);

            var nearbyPoints = await _mapService.GetNearbyMapPointsAsync(
                mapPointDto.Latitude,
                mapPointDto.Longitude,
                10,
                null,
                currentUserId);

            var viewModel = new MapPointDetailsViewModel
            {
                MapPoint = mapPointDto,
                Comments = comments,
                NearbyPoints = nearbyPoints.Where(p => p.Id != mapPointDto.Id).Take(5).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading map point {Slug}", slug);
            TempData["Error"] = _localizer["FailedToLoadMapPoint"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Maps/Create
    [Authorize]
    [HttpGet("Create")]
    public IActionResult Create()
    {
        ViewBag.Types = Enum.GetValues<MapPointType>();
        return View();
    }

    // POST: Maps/Create
    [Authorize]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMapPointViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Types = Enum.GetValues<MapPointType>();
                return View(model);
            }

            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();

            var mapPoint = await _mapService.CreateMapPointAsync(
                model.Name,
                model.Latitude,
                model.Longitude,
                model.Address,
                model.Type,
                userId,
                model.Description);

            TempData["Success"] = _localizer["MapPointCreated"].Value;
            return RedirectToAction(nameof(Details), new { slug = mapPoint.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating map point");
            ModelState.AddModelError("", _localizer["FailedToCreateMapPoint"].Value);
            ViewBag.Types = Enum.GetValues<MapPointType>();
            return View(model);
        }
    }

    // GET: Maps/Edit/{id}
    [Authorize]
    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            var mapPointDto = await _mapService.GetMapPointByIdAsync(id, userId);

            if (mapPointDto == null)
            {
                TempData["Error"] = _localizer["MapPointNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            if (!mapPointDto.IsOwner)
            {
                TempData["Error"] = _localizer["OnlyOwnerCanEditMapPoint"].Value;
                return RedirectToAction(nameof(Details), new { slug = mapPointDto.Slug });
            }

            var model = new EditMapPointViewModel
            {
                Id = mapPointDto.Id,
                Name = mapPointDto.Name,
                Description = mapPointDto.Description,
                Latitude = mapPointDto.Latitude,
                Longitude = mapPointDto.Longitude,
                Address = mapPointDto.Address,
                Type = mapPointDto.Type,
                PhoneNumber = mapPointDto.PhoneNumber,
                Website = mapPointDto.Website,
                ImageUrl = mapPointDto.ImageUrl
            };

            ViewBag.Types = Enum.GetValues<MapPointType>();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading map point for edit {MapPointId}", id);
            TempData["Error"] = _localizer["FailedToLoadMapPoint"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Maps/Edit/{id}
    [Authorize]
    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditMapPointViewModel model)
    {
        try
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Types = Enum.GetValues<MapPointType>();
                return View(model);
            }

            var mapPoint = await _mapService.UpdateMapPointAsync(
                id,
                model.Name,
                model.Latitude,
                model.Longitude,
                model.Address,
                model.Type,
                model.Description);

            TempData["Success"] = _localizer["MapPointUpdated"].Value;
            return RedirectToAction(nameof(Details), new { slug = mapPoint.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating map point {MapPointId}", id);
            ModelState.AddModelError("", _localizer["FailedToUpdateMapPoint"].Value);
            ViewBag.Types = Enum.GetValues<MapPointType>();
            return View(model);
        }
    }

    // POST: Maps/Delete/{id}
    [Authorize]
    [HttpPost("Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _mapService.DeleteMapPointAsync(id);
            return Json(new { success = true, message = _localizer["MapPointDeleted"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting map point {MapPointId}", id);
            return Json(new { success = false, message = _localizer["FailedToDeleteMapPoint"].Value });
        }
    }

    // POST: Maps/ToggleFavorite/{id}
    [Authorize]
    [HttpPost("ToggleFavorite/{id:guid}")]
    public async Task<IActionResult> ToggleFavorite(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _mapService.ToggleFavoriteAsync(id, userId);

            return Json(new { success = true, message = _localizer["FavoriteToggled"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling favorite for map point {MapPointId}", id);
            return Json(new { success = false, message = _localizer["FailedToToggleFavorite"].Value });
        }
    }

    // POST: Maps/CheckIn/{id}
    [Authorize]
    [HttpPost("CheckIn/{id:guid}")]
    public async Task<IActionResult> CheckIn(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _mapService.CheckInAsync(id, userId);

            return Json(new { success = true, message = _localizer["CheckedIn"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in to map point {MapPointId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Maps/Rate/{id}
    [Authorize]
    [HttpPost("Rate/{id:guid}")]
    public async Task<IActionResult> Rate(Guid id, int rating, string? comment = null)
    {
        try
        {
            if (rating < 1 || rating > 5)
                return Json(new { success = false, message = _localizer["RatingLimit"].Value });

            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _mapService.AddOrUpdateRatingAsync(id, userId, rating, comment);

            return Json(new { success = true, message = _localizer["RatingSubmitted"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rating map point {MapPointId}", id);
            return Json(new { success = false, message = _localizer["FailedToSubmitRating"].Value });
        }
    }

    // POST: Maps/DeleteRating/{id}
    [Authorize]
    [HttpPost("DeleteRating/{id:guid}")]
    public async Task<IActionResult> DeleteRating(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _mapService.DeleteRatingAsync(id, userId);

            return Json(new { success = true, message = _localizer["RatingDeleted"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting rating for map point {MapPointId}", id);
            return Json(new { success = false, message = _localizer["FailedToDeleteRating"].Value });
        }
    }

    // POST: Maps/AddComment
    [Authorize]
    [HttpPost("AddComment")]
    public async Task<IActionResult> AddComment(Guid mapPointId, string content)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _mapService.AddCommentAsync(mapPointId, userId, content);

            return Json(new { success = true, message = _localizer["CommentAdded"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to map point {MapPointId}", mapPointId);
            return Json(new { success = false, message = _localizer["FailedToAddComment"].Value });
        }
    }

    // GET: Maps/Search
    [HttpGet("Search")]
    public async Task<IActionResult> Search(
        string? searchTerm,
        double? latitude,
        double? longitude,
        double radiusKm = 10,
        MapPointType? type = null,
        int page = 1,
        int pageSize = 12)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };

            PagedResult<Domain.DTOs.Community.MapPointDto> result;

            if (latitude.HasValue && longitude.HasValue)
            {
                var nearbyPoints = await _mapService.GetNearbyMapPointsAsync(
                    latitude.Value,
                    longitude.Value,
                    radiusKm,
                    type,
                    currentUserId);

                result = new PagedResult<Domain.DTOs.Community.MapPointDto>(
                    nearbyPoints.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                    nearbyPoints.Count,
                    page,
                    pageSize);
            }
            else if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                result = await _mapService.SearchMapPointsAsync(searchTerm, parameters, currentUserId);
            }
            else
            {
                result = await _mapService.GetMapPointsAsync(
                    parameters,
                    MapPointStatus.Published,
                    type,
                    null,
                    currentUserId);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Latitude = latitude;
            ViewBag.Longitude = longitude;
            ViewBag.RadiusKm = radiusKm;
            ViewBag.Type = type;
            ViewBag.Types = Enum.GetValues<MapPointType>();

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching map points");
            TempData["Error"] = _localizer["FailedToSearchMapPoints"].Value;
            return View(new PagedResult<Domain.DTOs.Community.MapPointDto>(
                new List<Domain.DTOs.Community.MapPointDto>(), 0, page, pageSize));
        }
    }

    // GET: Maps/Nearby
    [HttpGet("Nearby")]
    public async Task<IActionResult> Nearby(
        double latitude,
        double longitude,
        double radiusKm = 10,
        MapPointType? type = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var nearbyPoints = await _mapService.GetNearbyMapPointsAsync(
                latitude,
                longitude,
                radiusKm,
                type,
                currentUserId);

            return Json(new { success = true, data = nearbyPoints });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting nearby map points");
            return Json(new { success = false, message = _localizer["FailedToGetNearbyMapPoints"].Value });
        }
    }

    // GET: Maps/Featured
    [HttpGet("Featured")]
    public async Task<IActionResult> Featured(int count = 10)
    {
        try
        {
            var featuredPoints = await _mapService.GetFeaturedMapPointsAsync(count);
            return View(featuredPoints);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading featured map points");
            TempData["Error"] = _localizer["FailedToLoadFeaturedMapPoints"].Value;
            return View(new List<Domain.DTOs.Community.MapPointDto>());
        }
    }

    // GET: Maps/MyPoints
    [Authorize]
    [HttpGet("MyPoints")]
    public async Task<IActionResult> MyPoints(int page = 1, int pageSize = 12)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var result = await _mapService.GetMapPointsAsync(
                parameters,
                null,
                null,
                userId,
                userId);

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user map points");
            TempData["Error"] = _localizer["FailedToLoadMyMapPoints"].Value;
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
