using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityCar.Domain.Interfaces.Dashboard.Monitoring.Health;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels.Monitoring.Health;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.health;

[Area("Dashboard")]
[Route("{culture}/Dashboard/Monitoring/Health")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class HealthController : Controller
{
    private readonly IHealthService _healthService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IHealthService healthService, ILogger<HealthController> logger)
    {
        _healthService = healthService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var systemHealth = await _healthService.GetSystemHealthAsync();
            var viewModel = new HealthViewModel
            {
                SystemHealth = systemHealth
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system health");
            return View("Error");
        }
    }

    [HttpGet("api/status")]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var systemHealth = await _healthService.GetSystemHealthAsync();
            return Json(new
            {
                status = systemHealth.OverallStatus.ToString(),
                checks = systemHealth.Checks,
                metrics = systemHealth.Metrics,
                lastChecked = systemHealth.LastChecked
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving health status");
            return StatusCode(500, new { error = "Failed to retrieve health status" });
        }
    }

    [HttpGet("api/database")]
    public async Task<IActionResult> CheckDatabase()
    {
        var result = await _healthService.CheckDatabaseHealthAsync();
        return Json(result);
    }

    [HttpGet("api/cache")]
    public async Task<IActionResult> CheckCache()
    {
        var result = await _healthService.CheckCacheHealthAsync();
        return Json(result);
    }

    [HttpGet("api/metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        var metrics = await _healthService.GetSystemMetricsAsync();
        return Json(metrics);
    }

    [HttpGet("api/history")]
    public async Task<IActionResult> GetHistory(int hours = 24)
    {
        var history = await _healthService.GetHealthHistoryAsync(hours);
        return Json(history);
    }
}
