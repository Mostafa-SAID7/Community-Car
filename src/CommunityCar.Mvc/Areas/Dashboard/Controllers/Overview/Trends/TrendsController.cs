using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels.Overview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.Overview.Trends;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Trends")]
public class TrendsController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IStringLocalizer<TrendsController> _localizer;

    public TrendsController(IDashboardService dashboardService, IStringLocalizer<TrendsController> localizer)
    {
        _dashboardService = dashboardService;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        var userGrowth = await _dashboardService.GetUserGrowthAsync();
        var weeklyActivity = await _dashboardService.GetWeeklyActivityAsync();
        var contentDistribution = await _dashboardService.GetContentDistributionAsync();
        
        var viewModel = new TrendsViewModel
        {
            UserGrowth = userGrowth.Select(u => new ChartDataViewModel(u.Label, u.Value)).ToList(),
            WeeklyActivity = weeklyActivity.Select(a => new ChartDataViewModel(a.Label, a.Value)).ToList(),
            ContentDistribution = contentDistribution.Select(c => new ChartDataViewModel(c.Label, c.Value)).ToList(),
            CurrentPeriod = "week"
        };
        
        return View("~/Areas/Dashboard/Views/Overview/Trends/Index.cshtml", viewModel);
    }
    
    [HttpGet("GetTrendData")]
    public async Task<IActionResult> GetTrendData(string period = "week", string type = "users")
    {
        try
        {
            if (type == "users")
            {
                var data = await _dashboardService.GetActivityByPeriodAsync(period);
                return Json(new TrendDataResponse
                {
                    Success = true,
                    Data = data.Select(d => d.Value).ToList(),
                    Labels = data.Select(d => d.Label).ToList()
                });
            }
            
            return Json(new TrendDataResponse { Success = false, Message = "Invalid type" });
        }
        catch (Exception ex)
        {
            return Json(new TrendDataResponse { Success = false, Message = ex.Message });
        }
    }
}
