using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels.Dashboard;

public class KPIValueViewModel
{
    [Required]
    [StringLength(50)]
    public string Label { get; set; } = string.Empty;

    [Required]
    public double Value { get; set; }
}
