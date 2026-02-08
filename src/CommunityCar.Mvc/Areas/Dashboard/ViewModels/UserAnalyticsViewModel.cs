namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels;

public class UserAnalyticsViewModel
{
    public string Period { get; set; } = "30days";
    public int TotalUsers { get; set; }
    public int NewUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int AdminCount { get; set; }
    public int SuperAdminCount { get; set; }
    public int ModeratorCount { get; set; }
    public int RegularUserCount { get; set; }
    public double AverageContentPerUser { get; set; }
    
    public Dictionary<string, int> UserGrowthData { get; set; } = new();
    public List<UserContributionDto> TopContributors { get; set; } = new();
    public List<RecentUserDto> RecentUsers { get; set; } = new();
}

public class UserContributionDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ContributionCount { get; set; }
}

public class RecentUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public bool EmailConfirmed { get; set; }
}
