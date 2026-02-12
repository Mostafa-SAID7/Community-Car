namespace CommunityCar.Domain.DTOs.Dashboard.Analytics;

public class ContentActivityDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string ContentId { get; set; } = string.Empty;
    public string ContentTitle { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ContentActivityStatistics
{
    public int TotalActivities { get; set; }
    public int UniqueUsers { get; set; }
    public int TotalViews { get; set; }
    public int TotalCreates { get; set; }
    public int TotalUpdates { get; set; }
    public int TotalDeletes { get; set; }
    public Dictionary<string, int> ActivitiesByContentType { get; set; } = new();
    public Dictionary<string, int> ActivitiesByAction { get; set; } = new();
    public List<TopUserActivity> TopUsers { get; set; } = new();
    public List<TopContentActivity> TopContent { get; set; } = new();
}

public class TopUserActivity
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int ActivityCount { get; set; }
}

public class TopContentActivity
{
    public string ContentType { get; set; } = string.Empty;
    public string ContentId { get; set; } = string.Empty;
    public string ContentTitle { get; set; } = string.Empty;
    public int ViewCount { get; set; }
}
