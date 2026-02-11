namespace CommunityCar.Domain.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalUsers { get; set; }
    public string? Slug { get; set; }
    public int TotalFriendships { get; set; }
    public int ActiveEvents { get; set; }
    public double SystemHealth { get; set; }
    
    // Additional metrics
    public int TotalPosts { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalGroups { get; set; }
    public int TotalReviews { get; set; }
    public int ActiveUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
    public double EngagementRate { get; set; }
}
