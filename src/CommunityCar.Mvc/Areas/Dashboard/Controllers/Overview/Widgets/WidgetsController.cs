using CommunityCar.Domain.Interfaces.Dashboard.Overview;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels.Overview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.Overview.Widgets;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Widgets")]
public class WidgetsController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IStringLocalizer<WidgetsController> _localizer;

    public WidgetsController(IDashboardService dashboardService, IStringLocalizer<WidgetsController> localizer)
    {
        _dashboardService = dashboardService;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        var recentActivity = await _dashboardService.GetRecentActivityAsync(5);
        var contentDistribution = await _dashboardService.GetContentDistributionAsync();
        var usersByLocation = await _dashboardService.GetUsersByLocationAsync();
        
        var viewModel = new WidgetsViewModel
        {
            Summary = summary,
            RecentActivity = recentActivity.ToList(),
            ContentDistribution = contentDistribution.Select(c => new ChartDataViewModel(c.Label, c.Value)).ToList(),
            UsersByLocation = usersByLocation
        };
        
        return View("~/Areas/Dashboard/Views/Overview/Widgets/Index.cshtml", viewModel);
    }
}
