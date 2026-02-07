using System.ComponentModel.DataAnnotations;
using CommunityCar.Domain.DTOs.Dashboard;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels;

public class LocalizationIndexViewModel
{
    public List<LocalizationResourceDto> Resources { get; set; } = new();
    public LocalizationFilterViewModel Filter { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
    public LocalizationStatisticsDto Statistics { get; set; } = new();
    public List<string> AvailableCategories { get; set; } = new();
    public List<string> AvailableCultures { get; set; } = new();
}

public class LocalizationFilterViewModel
{
    public string? Key { get; set; }
    public string? Category { get; set; }
    public string? CultureCode { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
}

public class LocalizationDetailsViewModel
{
    public LocalizationResourceDto Resource { get; set; } = new();
    public List<LocalizationResourceDto> RelatedTranslations { get; set; } = new();
}

public class CreateLocalizationViewModel
{
    [Required(ErrorMessage = "Key is required")]
    [StringLength(200, ErrorMessage = "Key cannot exceed 200 characters")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Culture code is required")]
    [StringLength(10, ErrorMessage = "Culture code cannot exceed 10 characters")]
    public string CultureCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Value is required")]
    public string Value { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

public class EditLocalizationViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CultureCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Value is required")]
    public string Value { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public bool IsActive { get; set; }
}

public class BulkImportViewModel
{
    [Required(ErrorMessage = "Culture code is required")]
    public string CultureCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "JSON content is required")]
    public string JsonContent { get; set; } = string.Empty;

    public bool OverwriteExisting { get; set; }
}

public class MissingTranslationsViewModel
{
    public string SourceCulture { get; set; } = string.Empty;
    public string TargetCulture { get; set; } = string.Empty;
    public List<LocalizationResourceDto> MissingResources { get; set; } = new();
}

public class LocalizationStatisticsViewModel
{
    public LocalizationStatisticsDto Statistics { get; set; } = new();
    public Dictionary<string, int> TranslationsByCulture { get; set; } = new();
    public Dictionary<string, int> TranslationsByCategory { get; set; } = new();
}

public class ExportLocalizationViewModel
{
    public List<string> SelectedCultures { get; set; } = new();
    public List<string> SelectedCategories { get; set; } = new();
    public string ExportFormat { get; set; } = "json";
    public List<string> AvailableCultures { get; set; } = new();
    public List<string> AvailableCategories { get; set; } = new();
}

public class SyncTranslationsViewModel
{
    [Required(ErrorMessage = "Source culture is required")]
    public string SourceCulture { get; set; } = string.Empty;

    [Required(ErrorMessage = "Target culture is required")]
    public string TargetCulture { get; set; } = string.Empty;

    public bool OverwriteExisting { get; set; }
    public List<string> AvailableCultures { get; set; } = new();
}
