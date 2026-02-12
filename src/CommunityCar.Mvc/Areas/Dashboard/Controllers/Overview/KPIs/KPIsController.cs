using CommunityCar.Domain.Interfaces.Dashboard.Overview;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels.Overview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.Overview.KPIs;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/KPIs")]
public class KPIsController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IStringLocalizer<KPIsController> _localizer;

    public KPIsController(IDashboardService dashboardService, IStringLocalizer<KPIsController> localizer)
    {
        _dashboardService = dashboardService;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        var contentDistribution = await _dashboardService.GetContentDistributionAsync();
        var engagementMetrics = await _dashboardService.GetEngagementMetricsAsync();
        var userGrowth = await _dashboardService.GetUserGrowthAsync();
        
        var viewModel = new KPIsViewModel
        {
            Summary = summary,
            ContentDistribution = contentDistribution.Select(c => new ChartDataViewModel(c.Label, c.Value)).ToList(),
            EngagementMetrics = engagementMetrics.Select(e => new ChartDataViewModel(e.Key, e.Value)).ToList(),
            UserGrowth = userGrowth.Select(u => new ChartDataViewModel(u.Label, u.Value)).ToList()
        };
        
        return View("~/Areas/Dashboard/Views/Overview/KPIs/Index.cshtml", viewModel);
    }
}
