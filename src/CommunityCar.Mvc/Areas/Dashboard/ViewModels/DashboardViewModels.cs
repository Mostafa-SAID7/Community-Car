using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels;

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
}

public class KPIValueViewModel
{
    [Required]
    [StringLength(50)]
    public string Label { get; set; } = string.Empty;

    [Required]
    public double Value { get; set; }
}
