namespace CommunityCar.Domain.DTOs.Dashboard.Overview;

public class KPIDto
{
    public Guid Id { get; set; }
    public string? Slug { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double? PreviousValue { get; set; }
    public double? ChangePercentage { get; set; }
    public string? Trend { get; set; } // "up", "down", "stable"
    public string? TrendDirection { get; set; } // Same as Trend for compatibility
    public string? Category { get; set; }
    public string? Description { get; set; }
    public double? Target { get; set; }
    public double? TargetAchievementPercentage { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset LastUpdated { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class KPIFilterDto
{
    public string? Category { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SortBy { get; set; } = "LastUpdated";
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class KPIStatisticsDto
{
    public int TotalKPIs { get; set; }
    public int TrendingUp { get; set; }
    public int TrendingDown { get; set; }
    public int Stable { get; set; }
    public Dictionary<string, int> KPIsByCategory { get; set; } = new();
    public List<KPIDto> TopPerformers { get; set; } = new();
    public List<KPIDto> NeedsAttention { get; set; } = new();
}

public class CreateKPIDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
}

public class UpdateKPIDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
}
