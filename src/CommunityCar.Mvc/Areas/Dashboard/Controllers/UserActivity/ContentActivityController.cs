using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;

namespace CommunityCar.Mvc.Areas.Dashboard.Controllers.UserActivity;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ContentActivityController : Controller
{
    private readonly ILogger<ContentActivityController> _logger;
    private readonly IContentActivityService _contentActivityService;

    public ContentActivityController(
        ILogger<ContentActivityController> logger,
        IContentActivityService contentActivityService)
    {
        _logger = logger;
        _contentActivityService = contentActivityService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? userId = null, string? contentType = null)
    {
        try
        {
            var activities = await _contentActivityService.GetContentActivitiesAsync(page, pageSize, userId, contentType);
            var viewModel = new ContentActivityViewModel
            {
                Activities = activities.Items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = activities.TotalCount,
                TotalPages = activities.TotalPages,
                UserId = userId,
                ContentType = contentType
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading content activities");
            TempData["Error"] = "Failed to load content activities";
            return View(new ContentActivityViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var activity = await _contentActivityService.GetContentActivityByIdAsync(id);
            if (activity == null)
            {
                TempData["Error"] = "Content activity not found";
                return RedirectToAction(nameof(Index));
            }

            return View(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading content activity details for ID: {Id}", id);
            TempData["Error"] = "Failed to load content activity details";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> UserActivities(string userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var activities = await _contentActivityService.GetUserContentActivitiesAsync(userId, page, pageSize);
            var viewModel = new ContentActivityViewModel
            {
                Activities = activities.Items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = activities.TotalCount,
                TotalPages = activities.TotalPages,
                UserId = userId
            };

            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user content activities for user: {UserId}", userId);
            TempData["Error"] = "Failed to load user content activities";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Statistics(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var statistics = await _contentActivityService.GetContentActivityStatisticsAsync(start, end);
            var viewModel = new ContentActivityStatisticsViewModel
            {
                Statistics = statistics,
                StartDate = start,
                EndDate = end
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading content activity statistics");
            TempData["Error"] = "Failed to load content activity statistics";
            return View(new ContentActivityStatisticsViewModel());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _contentActivityService.DeleteContentActivityAsync(id);
            if (result)
            {
                TempData["Success"] = "Content activity deleted successfully";
            }
            else
            {
                TempData["Error"] = "Failed to delete content activity";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting content activity: {Id}", id);
            TempData["Error"] = "An error occurred while deleting the content activity";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkDelete(int[] ids)
    {
        try
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["Error"] = "No activities selected";
                return RedirectToAction(nameof(Index));
            }

            var result = await _contentActivityService.BulkDeleteContentActivitiesAsync(ids);
            TempData["Success"] = $"Successfully deleted {result} content activities";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting content activities");
            TempData["Error"] = "An error occurred while deleting content activities";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Export(DateTime? startDate = null, DateTime? endDate = null, string? format = "csv")
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var data = await _contentActivityService.ExportContentActivitiesAsync(start, end, format);
            var fileName = $"content_activities_{start:yyyyMMdd}_{end:yyyyMMdd}.{format}";
            var contentType = format?.ToLower() == "csv" ? "text/csv" : "application/json";

            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting content activities");
            TempData["Error"] = "Failed to export content activities";
            return RedirectToAction(nameof(Index));
        }
    }
}
