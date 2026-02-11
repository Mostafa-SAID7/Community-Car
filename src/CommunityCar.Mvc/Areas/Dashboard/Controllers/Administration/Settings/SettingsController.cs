using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Enums.Dashboard.settings;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.settings;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Administration/Settings")]
public class SettingsController : Controller
{
    private readonly ISystemSettingService _systemSettingService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        ISystemSettingService systemSettingService,
        ICurrentUserService currentUserService,
        ILogger<SettingsController> logger)
    {
        _systemSettingService = systemSettingService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        SettingCategory? category = null,
        SettingDataType? dataType = null,
        bool? isReadOnly = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDescending = false,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var filter = new SystemSettingFilterDto
            {
                Category = category,
                DataType = dataType,
                IsReadOnly = isReadOnly,
                SearchTerm = searchTerm,
                SortBy = sortBy ?? "DisplayOrder",
                SortDescending = sortDescending,
                Page = page,
                PageSize = pageSize
            };

            var result = await _systemSettingService.GetSettingsAsync(filter);
            var summary = await _systemSettingService.GetSummaryAsync();

            var viewModel = new SettingsIndexViewModel
            {
                Settings = result.Items,
                Filter = new SettingsFilterViewModel
                {
                    Category = category,
                    DataType = dataType,
                    IsReadOnly = isReadOnly,
                    SearchTerm = searchTerm,
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
                return PartialView("_SettingsList", viewModel);
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system settings");
            TempData["Error"] = "Failed to load system settings. Please try again.";
            return View(new SettingsIndexViewModel());
        }
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var setting = await _systemSettingService.GetSettingByIdAsync(id);

            if (setting == null)
            {
                TempData["Error"] = "System setting not found.";
                return RedirectToAction(nameof(Index));
            }

            var relatedSettings = await _systemSettingService.GetSettingsAsync(new SystemSettingFilterDto
            {
                Category = setting.Category,
                Page = 1,
                PageSize = 5
            });

            var viewModel = new SettingsDetailsViewModel
            {
                Setting = setting,
                RelatedSettings = relatedSettings.Items.Where(s => s.Id != id).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system setting details for {Id}", id);
            TempData["Error"] = "Failed to load system setting details.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new CreateSystemSettingViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSystemSettingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var dto = new CreateSystemSettingDto
            {
                Key = model.Key,
                Value = model.Value,
                Category = model.Category,
                DataType = model.DataType,
                Description = model.Description,
                DefaultValue = model.DefaultValue,
                IsReadOnly = model.IsReadOnly,
                DisplayOrder = model.DisplayOrder
            };

            var setting = await _systemSettingService.CreateSettingAsync(dto);

            TempData["Success"] = $"System setting '{setting.Key}' created successfully.";
            return RedirectToAction(nameof(Details), new { id = setting.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating system setting");
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var setting = await _systemSettingService.GetSettingByIdAsync(id);

            if (setting == null)
            {
                TempData["Error"] = "System setting not found.";
                return RedirectToAction(nameof(Index));
            }

            if (setting.IsReadOnly)
            {
                TempData["Warning"] = "This setting is read-only and cannot be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var viewModel = new EditSystemSettingViewModel
            {
                Id = setting.Id,
                Key = setting.Key,
                Value = setting.Value,
                Category = setting.Category,
                DataType = setting.DataType,
                Description = setting.Description,
                DisplayOrder = setting.DisplayOrder
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit system setting form for {Id}", id);
            TempData["Error"] = "Failed to load system setting.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditSystemSettingViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var dto = new UpdateSystemSettingDto
            {
                Id = model.Id,
                Value = model.Value,
                Description = model.Description,
                DisplayOrder = model.DisplayOrder
            };

            await _systemSettingService.UpdateSettingAsync(dto);

            TempData["Success"] = "System setting updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system setting {Id}", id);
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _systemSettingService.DeleteSettingAsync(id);

            TempData["Success"] = "System setting deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting system setting {Id}", id);
            TempData["Error"] = "Failed to delete system setting.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("ResetToDefault/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetToDefault(Guid id)
    {
        try
        {
            await _systemSettingService.ResetToDefaultAsync(id);

            TempData["Success"] = "System setting reset to default value successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting system setting {Id}", id);
            TempData["Error"] = "Failed to reset system setting.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpGet("Category/{category}")]
    public async Task<IActionResult> Category(SettingCategory category)
    {
        return RedirectToAction(nameof(Index), new { category });
    }

    [HttpPost("BulkUpdate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkUpdate([FromBody] Dictionary<string, string> updates)
    {
        try
        {
            await _systemSettingService.BulkUpdateSettingsAsync(updates);

            return Json(new { success = true, message = "Settings updated successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating settings");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("Api/Summary")]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var summary = await _systemSettingService.GetSummaryAsync();
            return Json(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings summary");
            return Json(new SettingsSummaryDto());
        }
    }

    [HttpGet("Api/GetByKey/{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        try
        {
            var setting = await _systemSettingService.GetSettingByKeyAsync(key);
            if (setting == null)
                return NotFound();

            return Json(setting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading setting by key {Key}", key);
            return NotFound();
        }
    }
}
