using System.Text;
using global::System.Text;
using System.Text.Json;
using CommunityCar.Domain.DTOs.Dashboard.Monitoring.Audit;
using CommunityCar.Domain.Interfaces.Dashboard.Monitoring.Audit;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels.Monitoring.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CommunityCar.Mvc.Areas.Dashboard.Controllers.Monitoring.Audit;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Monitoring/Audit")]
public class AuditLogsController : Controller
{
    private readonly IAuditLogService _auditLogService;
    private readonly IStringLocalizer<AuditLogsController> _localizer;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(
        IAuditLogService auditLogService, 
        IStringLocalizer<AuditLogsController> localizer,
        ILogger<AuditLogsController> logger)
    {
        _auditLogService = auditLogService;
        _localizer = localizer;
        _logger = logger;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        string? userName = null,
        string? entityName = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var filter = new AuditLogFilterDto
            {
                UserName = userName,
                EntityName = entityName,
                Action = action,
                StartDate = startDate,
                EndDate = endDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _auditLogService.GetAuditLogsAsync(filter);
            var entities = await _auditLogService.GetDistinctEntityNamesAsync();
            var actions = await _auditLogService.GetDistinctActionsAsync();
            var stats = await _auditLogService.GetAuditLogStatisticsAsync(30);

            var viewModel = new AuditLogIndexViewModel
            {
                AuditLogs = result.Items,
                Filter = new AuditLogFilterViewModel
                {
                    UserName = userName,
                    EntityName = entityName,
                    Action = action,
                    StartDate = startDate,
                    EndDate = endDate,
                    AvailableEntities = entities,
                    AvailableActions = actions
                },
                Pagination = new PaginationViewModel
                {
                    CurrentPage = result.PageNumber,
                    TotalPages = result.TotalPages,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                },
                Statistics = new AuditLogStatisticsViewModel
                {
                    TotalLogs = result.TotalCount,
                    ActionCounts = stats
                }
            };

            return View("~/Areas/Dashboard/Views/Monitoring/Audit/Index.cshtml", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit logs");
            TempData["Error"] = _localizer["FailedToLoadLogs"].Value;
            return View("~/Areas/Dashboard/Views/Monitoring/Audit/Index.cshtml", new AuditLogIndexViewModel());
        }
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var auditLog = await _auditLogService.GetAuditLogByIdAsync(id);
            
            if (auditLog == null)
            {
                TempData["Error"] = _localizer["LogNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new AuditLogDetailsViewModel
            {
                AuditLog = auditLog
            };

            // Parse JSON values if available
            if (!string.IsNullOrWhiteSpace(auditLog.OldValues))
            {
                try
                {
                    viewModel.OldValuesDict = JsonSerializer.Deserialize<Dictionary<string, string>>(auditLog.OldValues);
                }
                catch
                {
                    // If not JSON, treat as plain text
                }
            }

            if (!string.IsNullOrWhiteSpace(auditLog.NewValues))
            {
                try
                {
                    viewModel.NewValuesDict = JsonSerializer.Deserialize<Dictionary<string, string>>(auditLog.NewValues);
                }
                catch
                {
                    // If not JSON, treat as plain text
                }
            }

            if (!string.IsNullOrWhiteSpace(auditLog.AffectedColumns))
            {
                viewModel.AffectedColumnsList = auditLog.AffectedColumns.Split(',').Select(c => c.Trim()).ToList();
            }

            return View("~/Areas/Dashboard/Views/Monitoring/Audit/Details.cshtml", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit log details for {Id}", id);
            TempData["Error"] = _localizer["FailedToLoadDetails"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Export")]
    public async Task<IActionResult> Export(
        string? userName = null,
        string? entityName = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var filter = new AuditLogFilterDto
            {
                UserName = userName,
                EntityName = entityName,
                Action = action,
                StartDate = startDate,
                EndDate = endDate,
                Page = 1,
                PageSize = 10000 // Get all for export
            };

            var result = await _auditLogService.GetAuditLogsAsync(filter);

            var csv = GenerateCsv(result.Items);
            var fileName = $"AuditLogs_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

            return File(global::System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting audit logs");
            TempData["Error"] = _localizer["FailedToExport"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Statistics")]
    public async Task<IActionResult> Statistics(int days = 30)
    {
        try
        {
            var stats = await _auditLogService.GetAuditLogStatisticsAsync(days);
            return Json(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading statistics");
            return Json(new Dictionary<string, int>());
        }
    }

    private string GenerateCsv(List<AuditLogDto> logs)
    {
        var csv = new global::System.Text.StringBuilder();
        csv.AppendLine("Id,UserName,EntityName,EntityId,Action,Description,CreatedAt");

        foreach (var log in logs)
        {
            csv.AppendLine($"\"{log.Id}\",\"{log.UserName}\",\"{log.EntityName}\",\"{log.EntityId}\",\"{log.Action}\",\"{log.Description}\",\"{log.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
        }

        return csv.ToString();
    }
}
