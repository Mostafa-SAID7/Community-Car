using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;

namespace CommunityCar.Mvc.Areas.Dashboard.Controllers.UserActivity;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/UserActivity")]
public class UserActivityController : Controller
{
    private readonly ILogger<UserActivityController> _logger;
    private readonly IUserActivityService _userActivityService;
    private readonly IStringLocalizer<UserActivityController> _localizer;

    public UserActivityController(
        ILogger<UserActivityController> logger,
        IUserActivityService userActivityService,
        IStringLocalizer<UserActivityController> localizer)
    {
        _logger = logger;
        _userActivityService = userActivityService;
        _localizer = localizer;
    }

    [HttpGet("")]
    [HttpGet("Index")]
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
            TempData["Error"] = _localizer["FailedToLoadActivities"].Value;
            return View(new UserActivityIndexViewModel());
        }
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var activity = await _userActivityService.GetUserActivityByIdAsync(id);
            if (activity == null)
            {
                TempData["Error"] = _localizer["ActivityNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            return View(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user activity details for ID: {Id}", id);
            TempData["Error"] = _localizer["FailedToLoadDetails"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("UserDetails")]
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
            TempData["Error"] = _localizer["FailedToLoadDetails"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Statistics")]
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
            TempData["Error"] = _localizer["FailedToLoadStatistics"].Value;
            return View(new UserActivityStatisticsViewModel());
        }
    }

    [HttpGet("Timeline")]
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
            TempData["Error"] = _localizer["FailedToLoadTimeline"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Export")]
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
            TempData["Error"] = _localizer["FailedToExport"].Value;
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
                TempData["Success"] = _localizer["ActivityDeleted"].Value;
            }
            else
            {
                TempData["Error"] = _localizer["FailedToDelete"].Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user activity: {Id}", id);
            TempData["Error"] = _localizer["ErrorOccurred"].Value;
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
                TempData["Error"] = _localizer["NoSelection"].Value;
                return RedirectToAction(nameof(Index));
            }

            var result = await _userActivityService.BulkDeleteUserActivitiesAsync(ids);
            TempData["Success"] = string.Format(_localizer["BulkDeleteSuccess"].Value, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting user activities");
            TempData["Error"] = _localizer["ErrorOccurred"].Value;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("ActiveUsers")]
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
            TempData["Error"] = _localizer["FailedToLoadActiveUsers"].Value;
            return View(new ActiveUsersViewModel());
        }
    }
}
