using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.trends;

[Area("Dashboard")]
[Route("{culture}/Dashboard/Trends")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class TrendsController : Controller
{
    private readonly IKPIService _kpiService;
    private readonly ISecurityAlertService _securityAlertService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<TrendsController> _logger;

    public TrendsController(
        IKPIService kpiService,
        ISecurityAlertService securityAlertService,
        IAuditLogService auditLogService,
        ILogger<TrendsController> logger)
    {
        _kpiService = kpiService;
        _securityAlertService = securityAlertService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(int days = 30)
    {
        try
        {
            var kpiSummary = await _kpiService.GetKPISummaryAsync();
            var securityStats = await _securityAlertService.GetStatisticsAsync(days);
            var auditStats = await _auditLogService.GetAuditLogStatisticsAsync(days);

            var viewModel = new TrendIndexViewModel
            {
                KPISummary = kpiSummary,
                SecurityStatistics = securityStats,
                AuditStatistics = auditStats,
                SelectedDays = days
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading trends overview");
            TempData["Error"] = "Failed to load trends. Please try again.";
            return View(new TrendIndexViewModel());
        }
    }

    [HttpGet("KPIs")]
    public async Task<IActionResult> KPIs(int days = 30)
    {
        try
        {
            var summary = await _kpiService.GetKPISummaryAsync();
            var allKPIs = await _kpiService.GetKPIsAsync(new Domain.DTOs.Dashboard.KPIFilterDto { PageSize = 1000 });

            var viewModel = new KPITrendsViewModel
            {
                Summary = summary,
                AllKPIs = allKPIs.Items.ToList(),
                SelectedDays = days
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading KPI trends");
            TempData["Error"] = "Failed to load KPI trends.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Security")]
    public async Task<IActionResult> Security(int days = 30)
    {
        try
        {
            var statistics = await _securityAlertService.GetStatisticsAsync(days);
            var trends = await _securityAlertService.GetAlertTrendsAsync(days);

            var viewModel = new SecurityTrendsViewModel
            {
                Statistics = statistics,
                Trends = trends,
                SelectedDays = days
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading security trends");
            TempData["Error"] = "Failed to load security trends.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Activity")]
    public async Task<IActionResult> Activity(int days = 30)
    {
        try
        {
            var auditStats = await _auditLogService.GetAuditLogStatisticsAsync(days);

            var viewModel = new ActivityTrendsViewModel
            {
                AuditStatistics = auditStats,
                SelectedDays = days
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading activity trends");
            TempData["Error"] = "Failed to load activity trends.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Api/KPIs")]
    public async Task<IActionResult> GetKPITrends(int days = 30)
    {
        try
        {
            var summary = await _kpiService.GetKPISummaryAsync();
            return Json(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading KPI trends");
            return StatusCode(500, new { error = "Failed to load KPI trends" });
        }
    }

    [HttpGet("Api/Security")]
    public async Task<IActionResult> GetSecurityTrends(int days = 30)
    {
        try
        {
            var trends = await _securityAlertService.GetAlertTrendsAsync(days);
            return Json(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading security trends");
            return StatusCode(500, new { error = "Failed to load security trends" });
        }
    }

    [HttpGet("Api/Activity")]
    public async Task<IActionResult> GetActivityTrends(int days = 30)
    {
        try
        {
            var stats = await _auditLogService.GetAuditLogStatisticsAsync(days);
            return Json(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading activity trends");
            return StatusCode(500, new { error = "Failed to load activity trends" });
        }
    }
}
