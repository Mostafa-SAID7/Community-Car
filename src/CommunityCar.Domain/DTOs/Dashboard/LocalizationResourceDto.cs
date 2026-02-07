namespace CommunityCar.Domain.DTOs.Dashboard;

public class LocalizationResourceDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CultureCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public class CreateLocalizationResourceDto
{
    public string Key { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CultureCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateLocalizationResourceDto
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class LocalizationFilterDto
{
    public string? Key { get; set; }
    public string? Category { get; set; }
    public string? CultureCode { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class LocalizationStatisticsDto
{
    public int TotalKeys { get; set; }
    public int TotalTranslations { get; set; }
    public Dictionary<string, int> TranslationsByCulture { get; set; } = new();
    public Dictionary<string, int> TranslationsByCategory { get; set; } = new();
    public int MissingTranslations { get; set; }
    public double CompletionPercentage { get; set; }
}

public class BulkImportDto
{
    public string CultureCode { get; set; } = string.Empty;
    public Dictionary<string, string> Translations { get; set; } = new();
    public bool OverwriteExisting { get; set; }
}


public class LocalizationResourceFilterDto
{
    public string? Key { get; set; }
    public string? Category { get; set; }
    public string? CultureCode { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
