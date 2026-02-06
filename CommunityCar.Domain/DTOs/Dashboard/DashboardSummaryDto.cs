namespace CommunityCar.Domain.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalUsers { get; set; }
    public string? Slug { get; set; }
    public int TotalFriendships { get; set; }
    public int ActiveEvents { get; set; }
    public double SystemHealth { get; set; }
}
