using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Domain.Models;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.overview;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Overview")]
public class OverviewController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IStringLocalizer<OverviewController> _localizer;

    public OverviewController(IDashboardService dashboardService, IStringLocalizer<OverviewController> localizer)
    {
        _dashboardService = dashboardService;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        var activity = await _dashboardService.GetWeeklyActivityAsync();
        
        var summaryViewModel = new DashboardSummaryViewModel
        {
            TotalUsers = summary.TotalUsers,
            TotalFriendships = summary.TotalFriendships,
            ActiveEvents = summary.ActiveEvents,
            SystemHealth = summary.SystemHealth,
            Slug = summary.Slug
        };

        ViewBag.ActivityData = activity.Select(a => a.Value).ToList();
        ViewBag.ActivityLabels = activity.Select(a => a.Label).ToList();
        
        return View(summaryViewModel);
    }

    [HttpGet("ExportReport")]
    public async Task<IActionResult> ExportReport(string format = "pdf")
    {
        try
        {
            var summary = await _dashboardService.GetSummaryAsync();
            var activity = await _dashboardService.GetWeeklyActivityAsync();

            if (format.ToLower() == "csv")
            {
                return await ExportToCsv(summary, activity);
            }
            else if (format.ToLower() == "json")
            {
                return await ExportToJson(summary, activity);
            }
            else
            {
                // Default to CSV for now (PDF would require additional library)
                return await ExportToCsv(summary, activity);
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Failed to export report: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<IActionResult> ExportToCsv(DashboardSummary summary, IEnumerable<KPIValue> activity)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Dashboard Report");
        csv.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        csv.AppendLine();
        csv.AppendLine("Summary Statistics");
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Users,{summary.TotalUsers}");
        csv.AppendLine($"Total Friendships,{summary.TotalFriendships}");
        csv.AppendLine($"Active Events,{summary.ActiveEvents}");
        csv.AppendLine($"System Health,{summary.SystemHealth}%");
        csv.AppendLine();
        csv.AppendLine("Weekly Activity");
        csv.AppendLine("Date,Registrations");
        
        foreach (var item in activity)
        {
            csv.AppendLine($"{item.Label},{item.Value}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        var fileName = $"dashboard-report-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
        
        return File(bytes, "text/csv", fileName);
    }

    private async Task<IActionResult> ExportToJson(DashboardSummary summary, IEnumerable<KPIValue> activity)
    {
        var report = new
        {
            GeneratedAt = DateTime.UtcNow,
            Summary = new
            {
                summary.TotalUsers,
                summary.TotalFriendships,
                summary.ActiveEvents,
                summary.SystemHealth
            },
            WeeklyActivity = activity.Select(a => new { a.Label, a.Value })
        };

        var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var fileName = $"dashboard-report-{DateTime.UtcNow:yyyyMMddHHmmss}.json";
        
        return File(bytes, "application/json", fileName);
    }

    [HttpGet("QuickActions")]
    public IActionResult QuickActions()
    {
        return View();
    }
}
