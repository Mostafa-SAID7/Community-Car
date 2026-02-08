using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Domain.Models;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.overview;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
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
}
