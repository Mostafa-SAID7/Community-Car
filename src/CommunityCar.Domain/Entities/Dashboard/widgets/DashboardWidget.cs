using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Dashboard.widgets;

public class DashboardWidget : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., "Chart", "Counter", "List"
    public string ConfigJson { get; set; } = "{}";
    public int Order { get; set; }
    public Guid UserId { get; set; } // User-specific dashboard layout
}
