using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;

namespace CommunityCar.Mvc.Areas.Dashboard.Controllers.UserActivity;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class UserActivityController : Controller
{
    private readonly ILogger<UserActivityController> _logger;
    private readonly IUserActivityService _userActivityService;

    public UserActivityController(
        ILogger<UserActivityController> logger,
        IUserActivityService userActivityService)
    {
        _logger = logger;
        _userActivityService = userActivityService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? activityType = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var result = await _userActivityService.GetUserActivitiesAsync(
                page, 
                pageSize, 
                searchTerm, 
                startDate, 
                endDate, 
                activityType);

            var viewModel = new UserActivityIndexViewModel
            {
                Activities = result.Items,
                Pagination = new PaginationViewModel
                {
                    CurrentPage = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount,
                    TotalPages = result.TotalPages
                },
                Filter = new UserActivityFilterViewModel
                {
                    SearchTerm = searchTerm,
                    StartDate = startDate,
                    EndDate = endDate,
                    ActivityType = activityType
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user activities");
            TempData["Error"] = "Failed to load user activities";
            return View(new UserActivityIndexViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var activity = await _userActivityService.GetUserActivityByIdAsync(id);
            if (activity == null)
            {
                TempData["Error"] = "User activity not found";
                return RedirectToAction(nameof(Index));
            }

            return View(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user activity details for ID: {Id}", id);
            TempData["Error"] = "Failed to load activity details";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> UserDetails(string userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var result = await _userActivityService.GetActivitiesByUserIdAsync(userId, page, pageSize);
            var summary = await _userActivityService.GetUserActivitySummaryAsync(userId);

            var viewModel = new UserActivityDetailsViewModel
            {
                UserId = userId,
                Activities = result.Items,
                Summary = summary,
                Pagination = new PaginationViewModel
                {
                    CurrentPage = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount,
                    TotalPages = result.TotalPages
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user activity details for user: {UserId}", userId);
            TempData["Error"] = "Failed to load user activity details";
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

            var statistics = await _userActivityService.GetUserActivityStatisticsAsync(start, end);
            var viewModel = new UserActivityStatisticsViewModel
            {
                Statistics = statistics,
                StartDate = start,
                EndDate = end
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user activity statistics");
            TempData["Error"] = "Failed to load statistics";
            return View(new UserActivityStatisticsViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Timeline(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            var timeline = await _userActivityService.GetUserActivityTimelineAsync(userId, start, end);
            var viewModel = new UserActivityTimelineViewModel
            {
                UserId = userId,
                Timeline = timeline,
                StartDate = start,
                EndDate = end
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user activity timeline for user: {UserId}", userId);
            TempData["Error"] = "Failed to load activity timeline";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Export(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? format = "csv")
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var data = await _userActivityService.ExportUserActivitiesAsync(start, end, format);
            var fileName = $"user_activities_{start:yyyyMMdd}_{end:yyyyMMdd}.{format}";
            var contentType = format?.ToLower() == "csv" ? "text/csv" : "application/json";

            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting user activities");
            TempData["Error"] = "Failed to export user activities";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _userActivityService.DeleteUserActivityAsync(id);
            if (result)
            {
                TempData["Success"] = "User activity deleted successfully";
            }
            else
            {
                TempData["Error"] = "Failed to delete user activity";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user activity: {Id}", id);
            TempData["Error"] = "An error occurred while deleting the activity";
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

            var result = await _userActivityService.BulkDeleteUserActivitiesAsync(ids);
            TempData["Success"] = $"Successfully deleted {result} user activities";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting user activities");
            TempData["Error"] = "An error occurred while deleting activities";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ActiveUsers(int hours = 24)
    {
        try
        {
            var activeUsers = await _userActivityService.GetActiveUsersAsync(hours);
            var viewModel = new ActiveUsersViewModel
            {
                Users = activeUsers,
                TimeRange = hours
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading active users");
            TempData["Error"] = "Failed to load active users";
            return View(new ActiveUsersViewModel());
        }
    }
}
