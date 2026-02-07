using CommunityCar.Domain.DTOs.Dashboard;
using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels;

public class WidgetIndexViewModel
{
    public List<WidgetDto> Widgets { get; set; } = new();
    public WidgetFilterViewModel Filter { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
    public Dictionary<string, int> TypeCounts { get; set; } = new();
}

public class WidgetFilterViewModel
{
    public Guid? UserId { get; set; }
    public string? Type { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

public class WidgetDetailsViewModel
{
    public WidgetDto Widget { get; set; } = new();
    public List<WidgetDto> UserWidgets { get; set; } = new();
}

public class CreateWidgetViewModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    public string ConfigJson { get; set; } = "{}";

    public int Order { get; set; }

    [Required]
    public Guid UserId { get; set; }
}

public class EditWidgetViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    public string ConfigJson { get; set; } = "{}";

    public int Order { get; set; }
}
