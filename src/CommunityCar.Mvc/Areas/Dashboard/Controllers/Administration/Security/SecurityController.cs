using CommunityCar.Domain.DTOs.Dashboard.Administration.Security;
using CommunityCar.Domain.Enums.Dashboard.security;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Dashboard.Administration.Security;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels.Administration.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.security;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Administration/Security")]
public class SecurityController : Controller
{
    private readonly ISecurityAlertService _securityAlertService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SecurityController> _logger;

    public SecurityController(
        ISecurityAlertService securityAlertService,
        ICurrentUserService currentUserService,
        ILogger<SecurityController> logger)
    {
        _securityAlertService = securityAlertService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        SecuritySeverity? severity = null,
        SecurityAlertType? alertType = null,
        bool? isResolved = null,
        string? searchTerm = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? sortBy = null,
        bool sortDescending = true,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var filter = new SecurityAlertFilterDto
            {
                Severity = severity,
                AlertType = alertType,
                IsResolved = isResolved,
                SearchTerm = searchTerm,
                StartDate = startDate,
                EndDate = endDate,
                SortBy = sortBy ?? "DetectedAt",
                SortDescending = sortDescending,
                Page = page,
                PageSize = pageSize
            };

            var result = await _securityAlertService.GetAlertsAsync(filter);
            var summary = await _securityAlertService.GetSummaryAsync();

            var viewModel = new SecurityIndexViewModel
            {
                Alerts = result.Items,
                Filter = new SecurityFilterViewModel
                {
                    Severity = severity,
                    AlertType = alertType,
                    IsResolved = isResolved,
                    SearchTerm = searchTerm,
                    StartDate = startDate,
                    EndDate = endDate,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                },
                Pagination = new PaginationViewModel
                {
                    CurrentPage = result.PageNumber,
                    TotalPages = result.TotalPages,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                },
                Summary = summary
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_AlertList", viewModel);
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading security alerts");
            TempData["Error"] = "Failed to load security alerts. Please try again.";
            return View(new SecurityIndexViewModel());
        }
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var alert = await _securityAlertService.GetAlertByIdAsync(id);

            if (alert == null)
            {
                TempData["Error"] = "Security alert not found.";
                return RedirectToAction(nameof(Index));
            }

            var relatedAlerts = await _securityAlertService.GetAlertsAsync(new SecurityAlertFilterDto
            {
                AlertType = alert.AlertType,
                Page = 1,
                PageSize = 5
            });

            var viewModel = new SecurityDetailsViewModel
            {
                Alert = alert,
                RelatedAlerts = relatedAlerts.Items.Where(a => a.Id != id).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading security alert details for {Id}", id);
            TempData["Error"] = "Failed to load security alert details.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new CreateSecurityAlertViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSecurityAlertViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var dto = new CreateSecurityAlertDto
            {
                Title = model.Title,
                Severity = model.Severity,
                AlertType = model.AlertType,
                Description = model.Description,
                Source = model.Source,
                IpAddress = model.IpAddress,
                UserAgent = model.UserAgent,
                AffectedUserId = model.AffectedUserId,
                AffectedUserName = model.AffectedUserName
            };

            var alert = await _securityAlertService.CreateAlertAsync(dto);

            TempData["Success"] = $"Security alert '{alert.Title}' created successfully.";
            return RedirectToAction(nameof(Details), new { id = alert.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating security alert");
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var alert = await _securityAlertService.GetAlertByIdAsync(id);

            if (alert == null)
            {
                TempData["Error"] = "Security alert not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new EditSecurityAlertViewModel
            {
                Id = alert.Id,
                Title = alert.Title,
                Severity = alert.Severity,
                AlertType = alert.AlertType,
                Description = alert.Description
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit security alert form for {Id}", id);
            TempData["Error"] = "Failed to load security alert.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditSecurityAlertViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var dto = new UpdateSecurityAlertDto
            {
                Id = model.Id,
                Title = model.Title,
                Severity = model.Severity,
                AlertType = model.AlertType,
                Description = model.Description
            };

            await _securityAlertService.UpdateAlertAsync(dto);

            TempData["Success"] = "Security alert updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating security alert {Id}", id);
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet("Resolve/{id}")]
    public async Task<IActionResult> Resolve(Guid id)
    {
        try
        {
            var alert = await _securityAlertService.GetAlertByIdAsync(id);

            if (alert == null)
            {
                TempData["Error"] = "Security alert not found.";
                return RedirectToAction(nameof(Index));
            }

            if (alert.IsResolved)
            {
                TempData["Warning"] = "This alert is already resolved.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var viewModel = new ResolveSecurityAlertViewModel
            {
                Id = alert.Id,
                Title = alert.Title,
                Severity = alert.Severity,
                Description = alert.Description,
                DetectedAt = alert.DetectedAt
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resolve form for {Id}", id);
            TempData["Error"] = "Failed to load security alert.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Resolve/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(Guid id, ResolveSecurityAlertViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var userId = _currentUserService.UserId ?? Guid.Empty;
            var userName = _currentUserService.UserName ?? "System";

            await _securityAlertService.ResolveAlertAsync(id, userId, userName, model.ResolutionNotes);

            TempData["Success"] = "Security alert resolved successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving security alert {Id}", id);
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpPost("Reopen/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reopen(Guid id)
    {
        try
        {
            await _securityAlertService.ReopenAlertAsync(id);

            TempData["Success"] = "Security alert reopened successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reopening security alert {Id}", id);
            TempData["Error"] = "Failed to reopen security alert.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _securityAlertService.DeleteAlertAsync(id);

            TempData["Success"] = "Security alert deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting security alert {Id}", id);
            TempData["Error"] = "Failed to delete security alert.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Statistics")]
    public async Task<IActionResult> Statistics()
    {
        try
        {
            var statistics = await _securityAlertService.GetStatisticsAsync();
            var trends = await _securityAlertService.GetAlertTrendsAsync(30);

            var viewModel = new SecurityStatisticsViewModel
            {
                Statistics = statistics,
                Trends = trends
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading security statistics");
            TempData["Error"] = "Failed to load statistics.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Unresolved")]
    public async Task<IActionResult> Unresolved()
    {
        try
        {
            var alerts = await _securityAlertService.GetUnresolvedAlertsAsync(50);
            return View(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading unresolved alerts");
            TempData["Error"] = "Failed to load unresolved alerts.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Critical")]
    public async Task<IActionResult> Critical()
    {
        try
        {
            var alerts = await _securityAlertService.GetCriticalAlertsAsync();
            return View(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading critical alerts");
            TempData["Error"] = "Failed to load critical alerts.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Api/Summary")]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var summary = await _securityAlertService.GetSummaryAsync();
            return Json(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading security summary");
            return Json(new SecuritySummaryDto());
        }
    }

    [HttpGet("Api/Trends")]
    public async Task<IActionResult> GetTrends(int days = 30)
    {
        try
        {
            var trends = await _securityAlertService.GetAlertTrendsAsync(days);
            return Json(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading alert trends");
            return Json(new Dictionary<string, int>());
        }
    }
}
