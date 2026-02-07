using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.DTOs.Dashboard;

public class WidgetDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ConfigJson { get; set; } = "{}";
    public int Order { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public class CreateWidgetDto
{
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ConfigJson { get; set; } = "{}";
    public int Order { get; set; }
    public Guid UserId { get; set; }
}

public class UpdateWidgetDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ConfigJson { get; set; } = "{}";
    public int Order { get; set; }
}

public class WidgetFilterDto
{
    public Guid? UserId { get; set; }
    public string? Type { get; set; }
    public string? SearchTerm { get; set; }
    public string SortBy { get; set; } = "Order";
    public bool SortDescending { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class WidgetConfigDto
{
    public string DataSource { get; set; } = string.Empty;
    public string ChartType { get; set; } = string.Empty;
    public Dictionary<string, object> Settings { get; set; } = new();
    public string RefreshInterval { get; set; } = "60";
}

public class WidgetTypeDto
{
    public string Type { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<string> AvailableDataSources { get; set; } = new();
}
