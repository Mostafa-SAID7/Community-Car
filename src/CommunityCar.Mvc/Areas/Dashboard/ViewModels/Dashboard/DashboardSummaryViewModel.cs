using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels.Dashboard;

public class DashboardSummaryViewModel
{
    [Required]
    public int TotalUsers { get; set; }
    
    public string? Slug { get; set; }

    [Required]
    public int TotalFriendships { get; set; }

    [Required]
    public int ActiveEvents { get; set; }

    [Required]
    [Range(0, 100)]
    public double SystemHealth { get; set; }
    
    // Additional metrics
    public int TotalPosts { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalGroups { get; set; }
    public int TotalReviews { get; set; }
    public int TotalGuides { get; set; }
    public int TotalNews { get; set; }
    public int ActiveUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
    
    [Range(0, 100)]
    public double EngagementRate { get; set; }
    
    public double UserGrowthPercentage { get; set; }
}
