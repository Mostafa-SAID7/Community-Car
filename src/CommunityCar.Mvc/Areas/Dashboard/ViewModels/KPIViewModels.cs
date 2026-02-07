using System.ComponentModel.DataAnnotations;
using CommunityCar.Domain.DTOs.Dashboard;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels;

public class KPIIndexViewModel
{
    public List<KPIDto> KPIs { get; set; } = new();
    public KPIFilterViewModel Filter { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
    public KPISummaryDto Summary { get; set; } = new();
    public List<string> Categories { get; set; } = new();
}

public class KPIFilterViewModel
{
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
    public List<string> AvailableCategories { get; set; } = new();
}

public class KPIDetailsViewModel
{
    public KPIDto KPI { get; set; } = new();
    public Dictionary<string, double> TrendData { get; set; } = new();
    public List<KPIDto> RelatedKPIs { get; set; } = new();
}

public class CreateKPIViewModel
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Code is required")]
    [StringLength(100, ErrorMessage = "Code cannot exceed 100 characters")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Code must contain only uppercase letters, numbers, and underscores")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Value is required")]
    public double Value { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    [StringLength(50, ErrorMessage = "Unit cannot exceed 50 characters")]
    public string Unit { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string? Category { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public List<string> AvailableCategories { get; set; } = new();
}

public class EditKPIViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Value is required")]
    public double Value { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    [StringLength(50, ErrorMessage = "Unit cannot exceed 50 characters")]
    public string Unit { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string? Category { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public List<string> AvailableCategories { get; set; } = new();
}

public class UpdateKPIValueViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double CurrentValue { get; set; }

    [Required(ErrorMessage = "New value is required")]
    public double NewValue { get; set; }

    public string Unit { get; set; } = string.Empty;
}

public class SetKPITargetViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double? CurrentTarget { get; set; }

    [Required(ErrorMessage = "Target is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Target must be non-negative")]
    public double Target { get; set; }

    public string Unit { get; set; } = string.Empty;
}
