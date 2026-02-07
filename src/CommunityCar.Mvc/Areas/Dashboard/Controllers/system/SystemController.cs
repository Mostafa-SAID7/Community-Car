using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.system;

[Area("Dashboard")]
[Route("Dashboard/System")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class SystemController : Controller
{
    private readonly ISystemService _systemService;
    private readonly ILogger<SystemController> _logger;

    public SystemController(ISystemService systemService, ILogger<SystemController> logger)
    {
        _systemService = systemService;
        _logger = logger;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var overview = await _systemService.GetSystemOverviewAsync();
            var recentLogs = await _systemService.GetRecentLogsAsync(50);
            var assemblyInfo = await _systemService.GetAssemblyInfoAsync();

            var viewModel = new SystemIndexViewModel
            {
                Overview = overview,
                RecentLogs = recentLogs,
                AssemblyInfo = assemblyInfo
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system overview");
            TempData["Error"] = "Failed to load system information. Please try again.";
            return View(new SystemIndexViewModel());
        }
    }

    [HttpGet("Info")]
    public async Task<IActionResult> Info()
    {
        try
        {
            var systemInfo = await _systemService.GetSystemInfoAsync();
            var environmentInfo = await _systemService.GetEnvironmentInfoAsync();
            var assemblyInfo = await _systemService.GetAssemblyInfoAsync();

            var viewModel = new SystemInfoViewModel
            {
                SystemInfo = systemInfo,
                EnvironmentInfo = environmentInfo,
                AssemblyInfo = assemblyInfo
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system information");
            TempData["Error"] = "Failed to load system information.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Resources")]
    public async Task<IActionResult> Resources()
    {
        try
        {
            var resources = await _systemService.GetSystemResourcesAsync();
            var processInfo = await _systemService.GetCurrentProcessInfoAsync();

            var viewModel = new SystemResourcesViewModel
            {
                Resources = resources,
                ProcessInfo = processInfo,
                MetricHistory = new List<SystemMetric>()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system resources");
            TempData["Error"] = "Failed to load system resources.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Services")]
    public async Task<IActionResult> Services()
    {
        try
        {
            var services = await _systemService.GetServicesStatusAsync();

            var viewModel = new SystemServicesViewModel
            {
                Services = services,
                LastChecked = DateTime.UtcNow
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system services");
            TempData["Error"] = "Failed to load system services.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Logs")]
    public async Task<IActionResult> Logs(string? level = null, int count = 100)
    {
        try
        {
            var logs = await _systemService.GetRecentLogsAsync(count, level);

            var viewModel = new SystemLogsViewModel
            {
                Logs = logs,
                FilterLevel = level,
                TotalCount = logs.Count
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system logs");
            TempData["Error"] = "Failed to load system logs.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Configuration")]
    public async Task<IActionResult> Configuration()
    {
        try
        {
            var configuration = await _systemService.GetConfigurationAsync();

            var viewModel = new SystemConfigurationViewModel
            {
                Configuration = configuration,
                ShowSensitiveData = false
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system configuration");
            TempData["Error"] = "Failed to load system configuration.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("ClearCache")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearCache()
    {
        try
        {
            var result = await _systemService.ClearCacheAsync();

            if (result)
            {
                TempData["Success"] = "Cache cleared successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to clear cache.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            TempData["Error"] = "Failed to clear cache.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Restart")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restart()
    {
        try
        {
            var result = await _systemService.RestartApplicationAsync();

            if (result)
            {
                TempData["Success"] = "Application restart initiated.";
            }
            else
            {
                TempData["Error"] = "Failed to restart application.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restarting application");
            TempData["Error"] = "Failed to restart application.";
            return RedirectToAction(nameof(Index));
        }
    }

    // API Endpoints
    [HttpGet("Api/Overview")]
    public async Task<IActionResult> GetOverview()
    {
        try
        {
            var overview = await _systemService.GetSystemOverviewAsync();
            return Json(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system overview");
            return StatusCode(500, new { error = "Failed to load system overview" });
        }
    }

    [HttpGet("Api/Metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        try
        {
            var metrics = await _systemService.GetSystemMetricsAsync();
            return Json(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system metrics");
            return StatusCode(500, new { error = "Failed to load system metrics" });
        }
    }

    [HttpGet("Api/Resources")]
    public async Task<IActionResult> GetResources()
    {
        try
        {
            var resources = await _systemService.GetSystemResourcesAsync();
            return Json(resources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system resources");
            return StatusCode(500, new { error = "Failed to load system resources" });
        }
    }

    [HttpGet("Api/Services")]
    public async Task<IActionResult> GetServices()
    {
        try
        {
            var services = await _systemService.GetServicesStatusAsync();
            return Json(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system services");
            return StatusCode(500, new { error = "Failed to load system services" });
        }
    }
}
