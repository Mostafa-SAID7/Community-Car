using AutoMapper;
using CommunityCar.Domain.DTOs.Dashboard.Overview;
using CommunityCar.Domain.Entities.Dashboard.widgets;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Dashboard.Overview;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CommunityCar.Infrastructure.Services.Dashboard.Overview;

public class WidgetService : IWidgetService
{
    private readonly IRepository<DashboardWidget> _widgetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<WidgetService> _logger;

    public WidgetService(
        IRepository<DashboardWidget> widgetRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<WidgetService> logger)
    {
        _widgetRepository = widgetRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<WidgetDto>> GetUserWidgetsAsync(Guid userId)
    {
        try
        {
            var widgets = await _widgetRepository
                .GetQueryable()
                .Where(w => w.UserId == userId)
                .OrderBy(w => w.Order)
                .ToListAsync();

            return _mapper.Map<List<WidgetDto>>(widgets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user widgets for user {UserId}", userId);
            throw;
        }
    }

    public async Task<WidgetDto?> GetWidgetByIdAsync(Guid id)
    {
        try
        {
            var widget = await _widgetRepository.GetByIdAsync(id);
            return widget != null ? _mapper.Map<WidgetDto>(widget) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting widget {WidgetId}", id);
            throw;
        }
    }

    public async Task<WidgetDto> CreateWidgetAsync(CreateWidgetDto dto, Guid userId)
    {
        try
        {
            var widget = new DashboardWidget
            {
                Title = dto.Title,
                Type = dto.Type,
                ConfigJson = dto.ConfigJson,
                Order = dto.Order,
                UserId = userId
            };

            await _widgetRepository.AddAsync(widget);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<WidgetDto>(widget);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating widget for user {UserId}", userId);
            throw;
        }
    }

    public async Task<WidgetDto> UpdateWidgetAsync(Guid id, UpdateWidgetDto dto)
    {
        try
        {
            var widget = await _widgetRepository.GetByIdAsync(id);
            if (widget == null)
            {
                throw new KeyNotFoundException($"Widget with ID {id} not found");
            }

            widget.Title = dto.Title;
            widget.ConfigJson = dto.ConfigJson;
            widget.Order = dto.Order;

            _widgetRepository.Update(widget);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<WidgetDto>(widget);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating widget {WidgetId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteWidgetAsync(Guid id)
    {
        try
        {
            var widget = await _widgetRepository.GetByIdAsync(id);
            if (widget == null)
            {
                return false;
            }

            _widgetRepository.Delete(widget);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting widget {WidgetId}", id);
            throw;
        }
    }

    public async Task<bool> ReorderWidgetsAsync(Guid userId, Dictionary<Guid, int> widgetOrders)
    {
        try
        {
            var widgets = await _widgetRepository
                .GetQueryable()
                .Where(w => w.UserId == userId && widgetOrders.Keys.Contains(w.Id))
                .ToListAsync();

            foreach (var widget in widgets)
            {
                if (widgetOrders.TryGetValue(widget.Id, out var newOrder))
                {
                    widget.Order = newOrder;
                    _widgetRepository.Update(widget);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering widgets for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<WidgetTypeDto>> GetAvailableWidgetTypesAsync()
    {
        return await Task.FromResult(new List<WidgetTypeDto>
        {
            new WidgetTypeDto
            {
                Type = "Chart",
                DisplayName = "Chart Widget",
                Description = "Display data in various chart formats (line, bar, pie, etc.)",
                Icon = "bi-graph-up",
                AvailableDataSources = new List<string> { "KPIs", "SecurityAlerts", "AuditLogs", "UserActivity" }
            },
            new WidgetTypeDto
            {
                Type = "Counter",
                DisplayName = "Counter Widget",
                Description = "Display a single numeric value with trend indicator",
                Icon = "bi-speedometer2",
                AvailableDataSources = new List<string> { "TotalUsers", "ActiveSessions", "PendingTasks", "SystemHealth" }
            },
            new WidgetTypeDto
            {
                Type = "List",
                DisplayName = "List Widget",
                Description = "Display a list of items (recent activities, alerts, etc.)",
                Icon = "bi-list-ul",
                AvailableDataSources = new List<string> { "RecentAlerts", "RecentLogs", "RecentUsers", "RecentQuestions" }
            },
            new WidgetTypeDto
            {
                Type = "Table",
                DisplayName = "Table Widget",
                Description = "Display data in a tabular format with sorting and filtering",
                Icon = "bi-table",
                AvailableDataSources = new List<string> { "Users", "SecurityAlerts", "AuditLogs", "KPIs" }
            },
            new WidgetTypeDto
            {
                Type = "Progress",
                DisplayName = "Progress Widget",
                Description = "Display progress towards a goal or target",
                Icon = "bi-bar-chart-fill",
                AvailableDataSources = new List<string> { "KPIProgress", "TaskCompletion", "SystemLoad" }
            },
            new WidgetTypeDto
            {
                Type = "Status",
                DisplayName = "Status Widget",
                Description = "Display system or service status indicators",
                Icon = "bi-check-circle",
                AvailableDataSources = new List<string> { "SystemHealth", "ServiceStatus", "DatabaseStatus" }
            }
        });
    }

    public async Task<object> GetWidgetDataAsync(Guid widgetId)
    {
        try
        {
            var widget = await _widgetRepository.GetByIdAsync(widgetId);
            if (widget == null)
            {
                throw new KeyNotFoundException($"Widget with ID {widgetId} not found");
            }

            // Parse config to determine data source
            var config = JsonSerializer.Deserialize<WidgetConfigDto>(widget.ConfigJson) ?? new WidgetConfigDto();

            // Return mock data based on widget type and data source
            // In a real implementation, this would fetch actual data from appropriate services
            return new
            {
                widgetId = widget.Id,
                type = widget.Type,
                dataSource = config.DataSource,
                data = GenerateMockData(widget.Type, config.DataSource),
                timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting widget data for {WidgetId}", widgetId);
            throw;
        }
    }

    public async Task<bool> DuplicateWidgetAsync(Guid widgetId)
    {
        try
        {
            var widget = await _widgetRepository.GetByIdAsync(widgetId);
            if (widget == null)
            {
                return false;
            }

            var duplicate = new DashboardWidget
            {
                Title = $"{widget.Title} (Copy)",
                Type = widget.Type,
                ConfigJson = widget.ConfigJson,
                Order = widget.Order + 1,
                UserId = widget.UserId
            };

            await _widgetRepository.AddAsync(duplicate);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating widget {WidgetId}", widgetId);
            throw;
        }
    }

    public async Task<bool> ResetUserDashboardAsync(Guid userId)
    {
        try
        {
            var widgets = await _widgetRepository
                .GetQueryable()
                .Where(w => w.UserId == userId)
                .ToListAsync();

            foreach (var widget in widgets)
            {
                _widgetRepository.Delete(widget);
            }

            await _unitOfWork.SaveChangesAsync();

            // Create default widgets
            await CreateDefaultWidgetsAsync(userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting dashboard for user {UserId}", userId);
            throw;
        }
    }

    private async Task CreateDefaultWidgetsAsync(Guid userId)
    {
        var defaultWidgets = new List<DashboardWidget>
        {
            new DashboardWidget
            {
                Title = "System Health",
                Type = "Status",
                ConfigJson = JsonSerializer.Serialize(new WidgetConfigDto { DataSource = "SystemHealth", RefreshInterval = "30" }),
                Order = 1,
                UserId = userId
            },
            new DashboardWidget
            {
                Title = "Active Users",
                Type = "Counter",
                ConfigJson = JsonSerializer.Serialize(new WidgetConfigDto { DataSource = "TotalUsers", RefreshInterval = "60" }),
                Order = 2,
                UserId = userId
            },
            new DashboardWidget
            {
                Title = "Recent Security Alerts",
                Type = "List",
                ConfigJson = JsonSerializer.Serialize(new WidgetConfigDto { DataSource = "RecentAlerts", RefreshInterval = "30" }),
                Order = 3,
                UserId = userId
            },
            new DashboardWidget
            {
                Title = "KPI Trends",
                Type = "Chart",
                ConfigJson = JsonSerializer.Serialize(new WidgetConfigDto { DataSource = "KPIs", ChartType = "line", RefreshInterval = "120" }),
                Order = 4,
                UserId = userId
            }
        };

        foreach (var widget in defaultWidgets)
        {
            await _widgetRepository.AddAsync(widget);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private object GenerateMockData(string widgetType, string dataSource)
    {
        return widgetType switch
        {
            "Counter" => new { value = Random.Shared.Next(100, 1000), trend = "up", change = "+12%" },
            "Chart" => new { labels = new[] { "Mon", "Tue", "Wed", "Thu", "Fri" }, values = new[] { 10, 20, 15, 25, 30 } },
            "List" => new[] { new { title = "Item 1", timestamp = DateTime.UtcNow }, new { title = "Item 2", timestamp = DateTime.UtcNow.AddMinutes(-5) } },
            "Status" => new { status = "Healthy", color = "success", message = "All systems operational" },
            _ => new { message = "No data available" }
        };
    }
}
