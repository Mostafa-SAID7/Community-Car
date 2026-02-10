using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.KPIs;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/KPIs")]
public class KPIsController : Controller
{
    private readonly IKPIService _kpiService;
    private readonly ILogger<KPIsController> _logger;

    public KPIsController(IKPIService kpiService, ILogger<KPIsController> logger)
    {
        _kpiService = kpiService;
        _logger = logger;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        string? category = null,
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
            var filter = new KPIFilterDto
            {
                Category = category,
                SearchTerm = searchTerm,
                StartDate = startDate,
                EndDate = endDate,
                SortBy = sortBy ?? "LastUpdated",
                SortDescending = sortDescending,
                Page = page,
                PageSize = pageSize
            };

            var result = await _kpiService.GetKPIsAsync(filter);
            var categories = await _kpiService.GetCategoriesAsync();
            var summary = await _kpiService.GetKPISummaryAsync();

            var viewModel = new KPIIndexViewModel
            {
                KPIs = result.Items,
                Filter = new KPIFilterViewModel
                {
                    Category = category,
                    SearchTerm = searchTerm,
                    StartDate = startDate,
                    EndDate = endDate,
                    SortBy = sortBy,
                    SortDescending = sortDescending,
                    IsActive = null,
                    AvailableCategories = categories
                },
                Pagination = new PaginationViewModel
                {
                    CurrentPage = result.PageNumber,
                    TotalPages = result.TotalPages,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                },
                Summary = summary,
                Categories = categories
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_KPIList", viewModel);
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading KPIs");
            TempData["Error"] = "Failed to load KPIs. Please try again.";
            return View(new KPIIndexViewModel());
        }
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var kpi = await _kpiService.GetKPIByIdAsync(id);
            
            if (kpi == null)
            {
                TempData["Error"] = "KPI not found.";
                return RedirectToAction(nameof(Index));
            }

            var trendData = await _kpiService.GetKPITrendsAsync(kpi.Code, 30);
            var relatedKPIs = kpi.Category != null 
                ? await _kpiService.GetKPIsByCategoryAsync(kpi.Category) 
                : new List<KPIDto>();

            var viewModel = new KPIDetailsViewModel
            {
                KPI = kpi,
                TrendData = trendData,
                RelatedKPIs = relatedKPIs.Where(k => k.Id != id).Take(5).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading KPI details for {Id}", id);
            TempData["Error"] = "Failed to load KPI details.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        try
        {
            var categories = await _kpiService.GetCategoriesAsync();
            var viewModel = new CreateKPIViewModel
            {
                AvailableCategories = categories
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create KPI form");
            TempData["Error"] = "Failed to load form.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateKPIViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableCategories = await _kpiService.GetCategoriesAsync();
            return View(model);
        }

        try
        {
            var dto = new CreateKPIDto
            {
                Name = model.Name,
                Code = model.Code.ToUpperInvariant(),
                Value = model.Value,
                Unit = model.Unit,
                Category = model.Category,
                Description = model.Description
            };

            var kpi = await _kpiService.CreateKPIAsync(dto);

            TempData["Success"] = $"KPI '{kpi.Name}' created successfully.";
            return RedirectToAction(nameof(Details), new { id = kpi.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating KPI");
            ModelState.AddModelError("", ex.Message);
            model.AvailableCategories = await _kpiService.GetCategoriesAsync();
            return View(model);
        }
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var kpi = await _kpiService.GetKPIByIdAsync(id);
            
            if (kpi == null)
            {
                TempData["Error"] = "KPI not found.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _kpiService.GetCategoriesAsync();
            var viewModel = new EditKPIViewModel
            {
                Id = kpi.Id,
                Name = kpi.Name,
                Code = kpi.Code,
                Value = kpi.Value,
                Unit = kpi.Unit,
                Category = kpi.Category,
                Description = kpi.Description,
                AvailableCategories = categories
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit KPI form for {Id}", id);
            TempData["Error"] = "Failed to load KPI.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditKPIViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            model.AvailableCategories = await _kpiService.GetCategoriesAsync();
            return View(model);
        }

        try
        {
            var dto = new UpdateKPIDto
            {
                Id = model.Id,
                Name = model.Name,
                Value = model.Value,
                Unit = model.Unit,
                Category = model.Category,
                Description = model.Description
            };

            await _kpiService.UpdateKPIAsync(dto);

            TempData["Success"] = "KPI updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating KPI {Id}", id);
            ModelState.AddModelError("", ex.Message);
            model.AvailableCategories = await _kpiService.GetCategoriesAsync();
            return View(model);
        }
    }

    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _kpiService.DeleteKPIAsync(id);

            TempData["Success"] = "KPI deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting KPI {Id}", id);
            TempData["Error"] = "Failed to delete KPI.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("UpdateValue/{id}")]
    public async Task<IActionResult> UpdateValue(Guid id)
    {
        try
        {
            var kpi = await _kpiService.GetKPIByIdAsync(id);
            
            if (kpi == null)
            {
                TempData["Error"] = "KPI not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new UpdateKPIValueViewModel
            {
                Id = kpi.Id,
                Name = kpi.Name,
                CurrentValue = kpi.Value,
                NewValue = kpi.Value,
                Unit = kpi.Unit
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading update value form for {Id}", id);
            TempData["Error"] = "Failed to load KPI.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("UpdateValue/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateValue(Guid id, UpdateKPIValueViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _kpiService.UpdateKPIValueAsync(id, model.NewValue);

            TempData["Success"] = "KPI value updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating KPI value {Id}", id);
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet("Statistics")]
    public async Task<IActionResult> Statistics()
    {
        try
        {
            var statistics = await _kpiService.GetStatisticsAsync();
            return Json(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading KPI statistics");
            return Json(new KPIStatisticsDto());
        }
    }

    [HttpGet("Category/{category}")]
    public async Task<IActionResult> Category(string category, int page = 1, int pageSize = 20)
    {
        try
        {
            var filter = new KPIFilterDto
            {
                Category = category,
                Page = page,
                PageSize = pageSize
            };

            var result = await _kpiService.GetKPIsAsync(filter);
            var categories = await _kpiService.GetCategoriesAsync();
            var summary = await _kpiService.GetKPISummaryAsync();

            var viewModel = new KPIIndexViewModel
            {
                KPIs = result.Items,
                Filter = new KPIFilterViewModel
                {
                    Category = category,
                    IsActive = null,
                    AvailableCategories = categories
                },
                Pagination = new PaginationViewModel
                {
                    CurrentPage = result.PageNumber,
                    TotalPages = result.TotalPages,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                },
                Summary = summary,
                Categories = categories
            };

            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading KPIs by category {Category}", category);
            TempData["Error"] = "Failed to load KPIs.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Trends/{code}")]
    public async Task<IActionResult> Trends(string code, int days = 30)
    {
        try
        {
            var trends = await _kpiService.GetKPITrendsAsync(code, days);
            return Json(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading trends for {Code}", code);
            return Json(new Dictionary<string, double>());
        }
    }

    [HttpPost("QuickUpdate")]
    public async Task<IActionResult> QuickUpdate([FromBody] UpdateKPIValueRequest request)
    {
        try
        {
            await _kpiService.UpdateKPIValueAsync(request.Id, request.Value);
            var kpi = await _kpiService.GetKPIByIdAsync(request.Id);
            
            return Json(new { success = true, kpi });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in quick update for {Id}", request.Id);
            return Json(new { success = false, message = ex.Message });
        }
    }
}

public class UpdateKPIValueRequest
{
    public Guid Id { get; set; }
    public double Value { get; set; }
}
