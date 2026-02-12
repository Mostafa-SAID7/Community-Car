namespace CommunityCar.Domain.DTOs.Dashboard.Analytics;

public class UserActivityDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Location { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class UserActivitySummary
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TotalActivities { get; set; }
    public DateTime? LastActivity { get; set; }
    public Dictionary<string, int> ActivitiesByType { get; set; } = new();
    public List<string> RecentActivities { get; set; } = new();
    public int LoginCount { get; set; }
    public int PageViewCount { get; set; }
    public Dictionary<string, int> MostUsedIpAddresses { get; set; } = new();
}

public class UserActivityStatistics
{
    public int TotalActivities { get; set; }
    public int UniqueUsers { get; set; }
    public Dictionary<string, int> ActivitiesByType { get; set; } = new();
    public Dictionary<string, int> ActivitiesByDay { get; set; } = new();
    public Dictionary<string, int> TopUsers { get; set; } = new();
    public int TotalLogins { get; set; }
    public int TotalPageViews { get; set; }
    public Dictionary<int, int> ActivitiesByHour { get; set; } = new();
    public Dictionary<string, int> DailyActivities { get; set; } = new();
    public List<TopActiveUser> TopActiveUsers { get; set; } = new();
    public List<PopularPage> PopularPages { get; set; } = new();
}

public class TopActiveUser
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int ActivityCount { get; set; }
    public DateTime LastActivity { get; set; }
}

public class PopularPage
{
    public string PageUrl { get; set; } = string.Empty;
    public int ViewCount { get; set; }
}

public class TimelineActivity
{
    public DateTime Timestamp { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

public class ActiveUserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime LastActivity { get; set; }
    public int ActivityCount { get; set; }
    public string? CurrentLocation { get; set; }
    public bool IsOnline { get; set; }
}
