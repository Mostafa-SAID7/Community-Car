using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Dashboard;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Mvc.Areas.Dashboard.Controllers.widgets;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Widgets")]
public class WidgetsController : Controller
{
    private readonly IWidgetService _widgetService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<WidgetsController> _logger;

    public WidgetsController(
        IWidgetService widgetService,
        ICurrentUserService currentUserService,
        ILogger<WidgetsController> logger)
    {
        _widgetService = widgetService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        Guid? userId = null,
        string? type = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDescending = false,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var filter = new WidgetFilterDto
            {
                UserId = userId,
                Type = type,
                SearchTerm = searchTerm,
                SortBy = sortBy ?? "Order",
                SortDescending = sortDescending,
                Page = page,
                PageSize = pageSize
            };

            // Get user widgets with filtering
            var allWidgets = await _widgetService.GetUserWidgetsAsync(userId ?? Guid.Empty);
            
            // Apply filters
            var filteredWidgets = allWidgets.AsEnumerable();
            if (!string.IsNullOrEmpty(type))
                filteredWidgets = filteredWidgets.Where(w => w.Type == type);
            if (!string.IsNullOrEmpty(searchTerm))
                filteredWidgets = filteredWidgets.Where(w => w.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            
            // Apply sorting
            filteredWidgets = sortBy?.ToLower() switch
            {
                "title" => sortDescending ? filteredWidgets.OrderByDescending(w => w.Title) : filteredWidgets.OrderBy(w => w.Title),
                "type" => sortDescending ? filteredWidgets.OrderByDescending(w => w.Type) : filteredWidgets.OrderBy(w => w.Type),
                "createdat" => sortDescending ? filteredWidgets.OrderByDescending(w => w.CreatedAt) : filteredWidgets.OrderBy(w => w.CreatedAt),
                _ => sortDescending ? filteredWidgets.OrderByDescending(w => w.Order) : filteredWidgets.OrderBy(w => w.Order)
            };
            
            // Pagination
            var totalCount = filteredWidgets.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var pagedWidgets = filteredWidgets.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            // Get type counts
            var typeCounts = allWidgets.GroupBy(w => w.Type).ToDictionary(g => g.Key, g => g.Count());

            var viewModel = new WidgetIndexViewModel
            {
                Widgets = pagedWidgets,
                Filter = new WidgetFilterViewModel
                {
                    UserId = userId,
                    Type = type,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                },
                Pagination = new PaginationViewModel
                {
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    TotalCount = totalCount
                },
                TypeCounts = typeCounts
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_WidgetList", viewModel);
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading widgets");
            TempData["Error"] = "Failed to load widgets. Please try again.";
            return View(new WidgetIndexViewModel());
        }
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var widget = await _widgetService.GetWidgetByIdAsync(id);

            if (widget == null)
            {
                TempData["Error"] = "Widget not found.";
                return RedirectToAction(nameof(Index));
            }

            var userWidgets = await _widgetService.GetUserWidgetsAsync(widget.UserId);

            var viewModel = new WidgetDetailsViewModel
            {
                Widget = widget,
                UserWidgets = userWidgets.Where(w => w.Id != id).Take(5).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading widget details for {Id}", id);
            TempData["Error"] = "Failed to load widget details.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        var currentUserId = _currentUserService.UserId ?? Guid.Empty;
        return View(new CreateWidgetViewModel { UserId = currentUserId });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWidgetViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var dto = new CreateWidgetDto
            {
                Title = model.Title,
                Type = model.Type,
                ConfigJson = model.ConfigJson,
                Order = model.Order,
                UserId = model.UserId
            };

            var currentUserId = _currentUserService.UserId ?? model.UserId;
            var widget = await _widgetService.CreateWidgetAsync(dto, currentUserId);

            TempData["Success"] = $"Widget '{widget.Title}' created successfully.";
            return RedirectToAction(nameof(Details), new { id = widget.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating widget");
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var widget = await _widgetService.GetWidgetByIdAsync(id);

            if (widget == null)
            {
                TempData["Error"] = "Widget not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new EditWidgetViewModel
            {
                Id = widget.Id,
                Title = widget.Title,
                Type = widget.Type,
                ConfigJson = widget.ConfigJson,
                Order = widget.Order
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit widget form for {Id}", id);
            TempData["Error"] = "Failed to load widget.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditWidgetViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var dto = new UpdateWidgetDto
            {
                Id = model.Id,
                Title = model.Title,
                Type = model.Type,
                ConfigJson = model.ConfigJson,
                Order = model.Order
            };

            await _widgetService.UpdateWidgetAsync(id, dto);

            TempData["Success"] = "Widget updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating widget {Id}", id);
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
            await _widgetService.DeleteWidgetAsync(id);

            TempData["Success"] = "Widget deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting widget {Id}", id);
            TempData["Error"] = "Failed to delete widget.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Reorder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder([FromBody] Dictionary<Guid, int> widgetOrders)
    {
        try
        {
            var userId = _currentUserService.UserId ?? Guid.Empty;
            await _widgetService.ReorderWidgetsAsync(userId, widgetOrders);

            return Json(new { success = true, message = "Widgets reordered successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering widgets");
            return Json(new { success = false, message = "Failed to reorder widgets." });
        }
    }

    [HttpGet("Api/UserWidgets/{userId}")]
    public async Task<IActionResult> GetUserWidgets(Guid userId)
    {
        try
        {
            var widgets = await _widgetService.GetUserWidgetsAsync(userId);
            return Json(widgets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user widgets for {UserId}", userId);
            return Json(new List<WidgetDto>());
        }
    }

    [HttpGet("Api/TypeCounts")]
    public async Task<IActionResult> GetTypeCounts(Guid? userId = null)
    {
        try
        {
            var widgets = await _widgetService.GetUserWidgetsAsync(userId ?? Guid.Empty);
            var counts = widgets.GroupBy(w => w.Type).ToDictionary(g => g.Key, g => g.Count());
            return Json(counts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading widget type counts");
            return Json(new Dictionary<string, int>());
        }
    }
}
