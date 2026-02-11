using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Domain.Models;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CommunityCar.Web.Areas.Dashboard.Controllers;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard")]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IReportExportService _reportExportService;
    private readonly IStringLocalizer<DashboardController> _localizer;

    public DashboardController(
        IDashboardService dashboardService, 
        IReportExportService reportExportService,
        IStringLocalizer<DashboardController> localizer)
    {
        _dashboardService = dashboardService;
        _reportExportService = reportExportService;
        _localizer = localizer;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        var activity = await _dashboardService.GetWeeklyActivityAsync();
        var contentDistribution = await _dashboardService.GetContentDistributionAsync();
        var userGrowth = await _dashboardService.GetUserGrowthAsync();
        var topContentTypes = await _dashboardService.GetTopContentTypesAsync();
        var engagementMetrics = await _dashboardService.GetEngagementMetricsAsync();
        
        var summaryViewModel = new DashboardSummaryViewModel
        {
            TotalUsers = summary.TotalUsers,
            TotalFriendships = summary.TotalFriendships,
            ActiveEvents = summary.ActiveEvents,
            SystemHealth = summary.SystemHealth,
            Slug = summary.Slug,
            TotalPosts = summary.TotalPosts,
            TotalQuestions = summary.TotalQuestions,
            TotalGroups = summary.TotalGroups,
            TotalReviews = summary.TotalReviews,
            TotalGuides = summary.TotalGuides,
            TotalNews = summary.TotalNews,
            ActiveUsersToday = summary.ActiveUsersToday,
            NewUsersThisWeek = summary.NewUsersThisWeek,
            NewUsersThisMonth = summary.NewUsersThisMonth,
            EngagementRate = summary.EngagementRate,
            UserGrowthPercentage = summary.UserGrowthPercentage
        };

        // Weekly Activity Chart Data
        ViewBag.ActivityData = activity.Select(a => a.Value).ToList();
        ViewBag.ActivityLabels = activity.Select(a => a.Label).ToList();
        
        // Content Distribution Chart Data (Pie/Doughnut)
        ViewBag.ContentData = contentDistribution.Select(c => c.Value).ToList();
        ViewBag.ContentLabels = contentDistribution.Select(c => c.Label).ToList();
        
        // User Growth Chart Data (Line)
        ViewBag.GrowthData = userGrowth.Select(u => u.Value).ToList();
        ViewBag.GrowthLabels = userGrowth.Select(u => u.Label).ToList();
        
        // Engagement Metrics Chart Data (Bar)
        ViewBag.EngagementData = engagementMetrics.Values.ToList();
        ViewBag.EngagementLabels = engagementMetrics.Keys.ToList();
        
        return View(summaryViewModel);
    }

    [HttpGet("ExportReport")]
    public async Task<IActionResult> ExportReport(string format = "csv")
    {
        try
        {
            var summary = await _dashboardService.GetSummaryAsync();
            var activity = await _dashboardService.GetWeeklyActivityAsync();

            byte[] fileBytes;
            string contentType;
            
            switch (format.ToLower())
            {
                case "csv":
                    fileBytes = await _reportExportService.ExportToCsvAsync(summary, activity);
                    contentType = "text/csv";
                    break;
                    
                case "json":
                    fileBytes = await _reportExportService.ExportToJsonAsync(summary, activity);
                    contentType = "application/json";
                    break;
                    
                case "pdf":
                    fileBytes = await _reportExportService.ExportToPdfAsync(summary, activity);
                    contentType = "application/pdf";
                    break;
                    
                default:
                    fileBytes = await _reportExportService.ExportToCsvAsync(summary, activity);
                    contentType = "text/csv";
                    format = "csv";
                    break;
            }

            var fileName = _reportExportService.GenerateFileName(format);
            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to export report: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("QuickActions")]
    public IActionResult QuickActions()
    {
        return View();
    }
    
    [HttpGet("RefreshData")]
    public async Task<IActionResult> RefreshData()
    {
        try
        {
            var summary = await _dashboardService.GetSummaryAsync();
            var engagementMetrics = await _dashboardService.GetEngagementMetricsAsync();
            
            return Json(new
            {
                success = true,
                data = new
                {
                    totalUsers = summary.TotalUsers,
                    totalFriendships = summary.TotalFriendships,
                    activeEvents = summary.ActiveEvents,
                    systemHealth = summary.SystemHealth,
                    totalPosts = summary.TotalPosts,
                    totalQuestions = summary.TotalQuestions,
                    totalGroups = summary.TotalGroups,
                    totalReviews = summary.TotalReviews,
                    activeUsersToday = summary.ActiveUsersToday,
                    engagementRate = summary.EngagementRate,
                    engagementMetrics
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
