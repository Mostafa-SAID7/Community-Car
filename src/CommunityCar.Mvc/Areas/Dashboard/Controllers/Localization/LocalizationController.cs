using System.Text;
using System.Text.Json;
using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.Localization;

[Area("Dashboard")]
[Route("Dashboard/Localization")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class LocalizationController : Controller
{
    private readonly ILocalizationService _localizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStringLocalizer<LocalizationController> _localizer;
    private readonly ILogger<LocalizationController> _logger;

    public LocalizationController(
        ILocalizationService localizationService,
        ICurrentUserService currentUserService,
        IStringLocalizer<LocalizationController> localizer,
        ILogger<LocalizationController> logger)
    {
        _localizationService = localizationService;
        _currentUserService = currentUserService;
        _localizer = localizer;
        _logger = logger;
    }

    [HttpGet]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        string? culture = null,
        string? category = null,
        string? searchTerm = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var filter = new LocalizationFilterDto
            {
                CultureCode = culture,
                Category = category,
                SearchTerm = searchTerm,
                IsActive = isActive,
                Page = page,
                PageSize = pageSize
            };

            var result = await _localizationService.GetResourcesAsync(filter);
            var statistics = await _localizationService.GetStatisticsAsync();
            var cultures = await _localizationService.GetAvailableCulturesAsync();
            var categories = await _localizationService.GetAvailableCategoriesAsync();

            var viewModel = new LocalizationIndexViewModel
            {
                Resources = result.Items,
                Filter = new LocalizationFilterViewModel
                {
                    CultureCode = culture,
                    Category = category,
                    SearchTerm = searchTerm,
                    IsActive = isActive
                },
                Pagination = new PaginationViewModel
                {
                    CurrentPage = result.PageNumber,
                    TotalPages = result.TotalPages,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                },
                Statistics = statistics,
                AvailableCultures = cultures,
                AvailableCategories = categories
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ResourceList", viewModel);
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading localization resources");
            TempData["Error"] = _localizer["FailedToLoadResources"].Value;
            return View(new LocalizationIndexViewModel());
        }
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var resource = await _localizationService.GetResourceByIdAsync(id);

            if (resource == null)
            {
                TempData["Error"] = _localizer["ResourceNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            var relatedTranslations = await _localizationService.GetResourcesAsync(new LocalizationFilterDto
            {
                SearchTerm = resource.Key,
                Page = 1,
                PageSize = 10
            });

            var viewModel = new LocalizationDetailsViewModel
            {
                Resource = resource,
                RelatedTranslations = relatedTranslations.Items.Where(r => r.Id != id).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading localization resource details for {Id}", id);
            TempData["Error"] = _localizer["FailedToLoadDetails"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var cultures = await _localizationService.GetAvailableCulturesAsync();
        var categories = await _localizationService.GetAvailableCategoriesAsync();

        ViewBag.AvailableCultures = cultures;
        ViewBag.AvailableCategories = categories;

        return View(new CreateLocalizationViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLocalizationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var cultures = await _localizationService.GetAvailableCulturesAsync();
            var categories = await _localizationService.GetAvailableCategoriesAsync();
            ViewBag.AvailableCultures = cultures;
            ViewBag.AvailableCategories = categories;
            return View(model);
        }

        try
        {
            var userId = _currentUserService.UserId ?? Guid.Empty;
            var userName = _currentUserService.UserName ?? "System";

            var dto = new CreateLocalizationResourceDto
            {
                Key = model.Key,
                CultureCode = model.CultureCode,
                Value = model.Value,
                Category = model.Category,
                Description = model.Description
            };

            var resource = await _localizationService.CreateResourceAsync(dto);

            TempData["Success"] = string.Format(_localizer["ResourceCreated"].Value, resource.Key);
            return RedirectToAction(nameof(Details), new { id = resource.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating localization resource");
            ModelState.AddModelError("", ex.Message);

            var cultures = await _localizationService.GetAvailableCulturesAsync();
            var categories = await _localizationService.GetAvailableCategoriesAsync();
            ViewBag.AvailableCultures = cultures;
            ViewBag.AvailableCategories = categories;

            return View(model);
        }
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var resource = await _localizationService.GetResourceByIdAsync(id);

            if (resource == null)
            {
                TempData["Error"] = "Localization resource not found.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _localizationService.GetAvailableCategoriesAsync();
            ViewBag.AvailableCategories = categories;

            var viewModel = new EditLocalizationViewModel
            {
                Id = resource.Id,
                Key = resource.Key,
                CultureCode = resource.CultureCode,
                Value = resource.Value,
                Category = resource.Category,
                Description = resource.Description,
                IsActive = resource.IsActive
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit form for {Id}", id);
            TempData["Error"] = _localizer["FailedToLoadResource"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditLocalizationViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            var categories = await _localizationService.GetAvailableCategoriesAsync();
            ViewBag.AvailableCategories = categories;
            return View(model);
        }

        try
        {
            var userId = _currentUserService.UserId ?? Guid.Empty;
            var userName = _currentUserService.UserName ?? "System";

            var dto = new UpdateLocalizationResourceDto
            {
                Id = model.Id,
                Value = model.Value,
                Description = model.Description,
                IsActive = model.IsActive
            };

            await _localizationService.UpdateResourceAsync(dto);

            TempData["Success"] = _localizer["ResourceUpdated"].Value;
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating localization resource {Id}", id);
            ModelState.AddModelError("", ex.Message);

            var categories = await _localizationService.GetAvailableCategoriesAsync();
            ViewBag.AvailableCategories = categories;

            return View(model);
        }
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _localizationService.DeleteResourceAsync(id);

            TempData["Success"] = _localizer["ResourceDeleted"].Value;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting localization resource {Id}", id);
            TempData["Error"] = _localizer["FailedToDelete"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("BulkImport")]
    public async Task<IActionResult> BulkImport()
    {
        var cultures = await _localizationService.GetAvailableCulturesAsync();
        ViewBag.AvailableCultures = cultures;

        return View(new BulkImportViewModel());
    }

    [HttpPost("BulkImport")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkImport(BulkImportViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var cultures = await _localizationService.GetAvailableCulturesAsync();
            ViewBag.AvailableCultures = cultures;
            return View(model);
        }

        try
        {
            var userId = _currentUserService.UserId ?? Guid.Empty;
            var userName = _currentUserService.UserName ?? "System";

            Dictionary<string, string> translations;

            if (model.Format.ToLower() == "json")
            {
                translations = JsonSerializer.Deserialize<Dictionary<string, string>>(model.ImportData)
                    ?? new Dictionary<string, string>();
            }
            else if (model.Format.ToLower() == "csv")
            {
                translations = ParseCsv(model.ImportData);
            }
            else
            {
                throw new InvalidOperationException("Unsupported format");
            }

            var dto = new BulkImportDto
            {
                CultureCode = model.Culture,
                Translations = translations,
                OverwriteExisting = model.OverwriteExisting
            };

            var importedCount = await _localizationService.BulkImportAsync(dto);

            TempData["Success"] = string.Format(_localizer["BulkImportSuccess"].Value, importedCount);
            return RedirectToAction(nameof(Index), new { culture = model.Culture });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing translations");
            ModelState.AddModelError("", ex.Message);

            var cultures = await _localizationService.GetAvailableCulturesAsync();
            ViewBag.AvailableCultures = cultures;

            return View(model);
        }
    }

    [HttpGet("Export")]
    public async Task<IActionResult> Export()
    {
        var cultures = await _localizationService.GetAvailableCulturesAsync();
        var categories = await _localizationService.GetAvailableCategoriesAsync();

        var viewModel = new ExportLocalizationViewModel
        {
            AvailableCultures = cultures,
            AvailableCategories = categories
        };

        return View(viewModel);
    }

    [HttpPost("Export")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Export(ExportLocalizationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var cultures = await _localizationService.GetAvailableCulturesAsync();
            var categories = await _localizationService.GetAvailableCategoriesAsync();
            model.AvailableCultures = cultures;
            model.AvailableCategories = categories;
            return View(model);
        }

        try
        {
            var resources = await _localizationService.ExportResourcesAsync(model.Culture, model.Category);

            byte[] fileContent;
            string fileName;
            string contentType;

            if (model.Format.ToLower() == "json")
            {
                var json = JsonSerializer.Serialize(resources, new JsonSerializerOptions { WriteIndented = true });
                fileContent = Encoding.UTF8.GetBytes(json);
                fileName = $"localization_{model.Culture}_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
                contentType = "application/json";
            }
            else if (model.Format.ToLower() == "csv")
            {
                var csv = GenerateCsv(resources);
                fileContent = Encoding.UTF8.GetBytes(csv);
                fileName = $"localization_{model.Culture}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
                contentType = "text/csv";
            }
            else
            {
                throw new InvalidOperationException("Unsupported format");
            }

            return File(fileContent, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting translations");
            TempData["Error"] = _localizer["FailedToExport"].Value;

            var cultures = await _localizationService.GetAvailableCulturesAsync();
            var categories = await _localizationService.GetAvailableCategoriesAsync();
            model.AvailableCultures = cultures;
            model.AvailableCategories = categories;

            return View(model);
        }
    }

    [HttpGet("Sync")]
    public async Task<IActionResult> Sync()
    {
        var cultures = await _localizationService.GetAvailableCulturesAsync();

        var viewModel = new SyncTranslationsViewModel
        {
            AvailableCultures = cultures
        };

        return View(viewModel);
    }

    [HttpPost("Sync")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sync(SyncTranslationsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var cultures = await _localizationService.GetAvailableCulturesAsync();
            model.AvailableCultures = cultures;
            return View(model);
        }

        try
        {
            var syncedCount = await _localizationService.SyncMissingKeysAsync(
                model.SourceCulture,
                model.TargetCulture,
                model.OverwriteExisting);

            TempData["Success"] = string.Format(_localizer["SyncSuccess"].Value, syncedCount, model.SourceCulture, model.TargetCulture);
            return RedirectToAction(nameof(Index), new { culture = model.TargetCulture });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing translations");
            ModelState.AddModelError("", ex.Message);

            var cultures = await _localizationService.GetAvailableCulturesAsync();
            model.AvailableCultures = cultures;

            return View(model);
        }
    }

    [HttpPost("SyncToFiles")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncToFiles()
    {
        try
        {
            await _localizationService.SyncToJsonFilesAsync();
            TempData["Success"] = _localizer["SyncToFilesSuccess"].Value;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing translations to files");
            TempData["Error"] = _localizer["SyncToFilesFailed"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("MissingTranslations")]
    public async Task<IActionResult> MissingTranslations(string? culture)
    {
        try
        {
            var targetCulture = culture ?? "es"; // Default to Spanish or let user choose? 
            // Better: If culture is null, show a selection view or default to first non-en culture
            
            if (string.IsNullOrEmpty(culture))
            {
                var cultures = await _localizationService.GetAvailableCulturesAsync();
                targetCulture = cultures.FirstOrDefault(c => c != "en") ?? "ar";
            }

            var defaultCulture = "en";
            var missingResources = await _localizationService.GetMissingTranslationsAsync(defaultCulture, targetCulture);
            
            // Group by category for the view model
            var missingByCategory = missingResources
                .GroupBy(r => r.Category)
                .ToDictionary(g => g.Key, g => g.Select(r => r.Key).ToList());

            var viewModel = new MissingTranslationsViewModel
            {
                Culture = targetCulture,
                MissingByCategory = missingByCategory,
                TotalMissing = missingResources.Count
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading missing translations for {Culture}", culture);
            TempData["Error"] = _localizer["FailedToLoadMissing"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Statistics")]
    public async Task<IActionResult> Statistics()
    {
        try
        {
            var statistics = await _localizationService.GetStatisticsAsync();
            
            // LocalizationStatisticsViewModel requires Dictionary properties too
            // Reuse logic from Service implementation if needed, or just map what we have
            
            var viewModel = new LocalizationStatisticsViewModel
            {
                Statistics = statistics,
                TranslationsByCulture = statistics.TranslationsByCulture,
                TranslationsByCategory = statistics.TranslationsByCategory
            };
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading statistics");
            TempData["Error"] = _localizer["FailedToLoadStatistics"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("RefreshCache")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefreshCache()
    {
        try
        {
            await _localizationService.RefreshCacheAsync();
            TempData["Success"] = _localizer["CacheRefreshed"].Value;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache");
            TempData["Error"] = _localizer["FailedToRefreshCache"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Api/Statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var statistics = await _localizationService.GetStatisticsAsync();
            return Json(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading statistics");
            return Json(new LocalizationStatisticsDto());
        }
    }

    [HttpGet("Api/Cultures")]
    public async Task<IActionResult> GetCultures()
    {
        try
        {
            var cultures = await _localizationService.GetAvailableCulturesAsync();
            return Json(cultures);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cultures");
            return Json(new List<string>());
        }
    }

    [HttpGet("Api/Resources/{targetCulture}")]
    public async Task<IActionResult> GetResourcesForCulture(string targetCulture)
    {
        try
        {
            var resources = await _localizationService.GetCachedResourcesAsync(targetCulture);
            return Json(resources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resources for culture {Culture}", targetCulture);
            return Json(new Dictionary<string, string>());
        }
    }

    private Dictionary<string, string> ParseCsv(string csv)
    {
        var result = new Dictionary<string, string>();
        var lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines.Skip(1)) // Skip header
        {
            var parts = line.Split(',');
            if (parts.Length >= 2)
            {
                var key = parts[0].Trim('"');
                var value = parts[1].Trim('"');
                result[key] = value;
            }
        }

        return result;
    }

    private string GenerateCsv(Dictionary<string, string> resources)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Key,Value");

        foreach (var resource in resources)
        {
            csv.AppendLine($"\"{resource.Key}\",\"{resource.Value}\"");
        }

        return csv.ToString();
    }
}
