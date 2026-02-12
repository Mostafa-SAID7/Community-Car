using CommunityCar.Domain.DTOs.Dashboard.Overview;

namespace CommunityCar.Domain.Interfaces.Dashboard.Overview;

public interface IWidgetService
{
    Task<List<WidgetDto>> GetUserWidgetsAsync(Guid userId);
    Task<WidgetDto?> GetWidgetByIdAsync(Guid id);
    Task<WidgetDto> CreateWidgetAsync(CreateWidgetDto dto, Guid userId);
    Task<WidgetDto> UpdateWidgetAsync(Guid id, UpdateWidgetDto dto);
    Task<bool> DeleteWidgetAsync(Guid id);
    Task<bool> ReorderWidgetsAsync(Guid userId, Dictionary<Guid, int> widgetOrders);
    Task<List<WidgetTypeDto>> GetAvailableWidgetTypesAsync();
    Task<object> GetWidgetDataAsync(Guid widgetId);
    Task<bool> DuplicateWidgetAsync(Guid widgetId);
    Task<bool> ResetUserDashboardAsync(Guid userId);
}
