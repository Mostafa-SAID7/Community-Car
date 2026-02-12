using System.ComponentModel.DataAnnotations;
using CommunityCar.Domain.DTOs.Dashboard.Administration.Settings;
using CommunityCar.Domain.Enums.Dashboard.settings;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels.Administration.Settings;

public class SettingsIndexViewModel
{
    public List<SystemSettingDto> Settings { get; set; } = new();
    public SettingsFilterViewModel Filter { get; set; } = new();
    public PaginationViewModel Pagination { get; set; } = new();
    public SettingsSummaryDto Summary { get; set; } = new();
}

public class SettingsFilterViewModel
{
    public SettingCategory? Category { get; set; }
    public SettingDataType? DataType { get; set; }
    public bool? IsReadOnly { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

public class SettingsDetailsViewModel
{
    public SystemSettingDto Setting { get; set; } = new();
    public List<SystemSettingDto> RelatedSettings { get; set; } = new();
}

public class CreateSystemSettingViewModel
{
    [Required(ErrorMessage = "Key is required")]
    [StringLength(100, ErrorMessage = "Key cannot exceed 100 characters")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Key can only contain letters, numbers, dots, underscores, and hyphens")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "Value is required")]
    [StringLength(2000, ErrorMessage = "Value cannot exceed 2000 characters")]
    public string Value { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public SettingCategory Category { get; set; }

    [Required(ErrorMessage = "Data type is required")]
    public SettingDataType DataType { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [StringLength(2000, ErrorMessage = "Default value cannot exceed 2000 characters")]
    public string? DefaultValue { get; set; }

    public bool IsReadOnly { get; set; } = false;

    [Range(0, 9999, ErrorMessage = "Display order must be between 0 and 9999")]
    public int DisplayOrder { get; set; } = 0;
}

public class EditSystemSettingViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Key is required")]
    [StringLength(100, ErrorMessage = "Key cannot exceed 100 characters")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "Value is required")]
    [StringLength(2000, ErrorMessage = "Value cannot exceed 2000 characters")]
    public string Value { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public SettingCategory Category { get; set; }

    [Required(ErrorMessage = "Data type is required")]
    public SettingDataType DataType { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Range(0, 9999, ErrorMessage = "Display order must be between 0 and 9999")]
    public int DisplayOrder { get; set; } = 0;
}

public class SettingsCategoryViewModel
{
    public SettingCategory Category { get; set; }
    public List<SystemSettingDto> Settings { get; set; } = new();
    public int TotalCount { get; set; }
}
