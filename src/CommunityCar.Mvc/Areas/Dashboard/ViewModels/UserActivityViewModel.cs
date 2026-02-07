using CommunityCar.Domain.DTOs.Dashboard;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels;

public class UserActivityIndexViewModel
{
    public List<UserActivityDto> Activities { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
    public UserActivityFilterViewModel Filter { get; set; } = new();
    public UserActivityStatsSummary Stats { get; set; } = new();
}

public class UserActivityFilterViewModel
{
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ActivityType { get; set; }
    public List<string> AvailableActivityTypes { get; set; } = new()
    {
        "Login",
        "Logout",
        "ProfileUpdate",
        "PasswordChange",
        "EmailVerification",
        "PageView",
        "Search",
        "Download",
        "Upload",
        "Settings",
        "Other"
    };
}

public class UserActivityStatsSummary
{
    public int TotalActivities { get; set; }
    public int UniqueUsers { get; set; }
    public int TodayActivities { get; set; }
    public Dictionary<string, int> ActivitiesByType { get; set; } = new();
}

public class UserActivityDetailsViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<UserActivityDto> Activities { get; set; } = new();
    public UserActivitySummary Summary { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
}

public class UserActivityStatisticsViewModel
{
    public UserActivityStatistics Statistics { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class UserActivityTimelineViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<TimelineActivity> Timeline { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ActiveUsersViewModel
{
    public List<ActiveUserInfo> Users { get; set; } = new();
    public int TimeRange { get; set; } = 24;
}
