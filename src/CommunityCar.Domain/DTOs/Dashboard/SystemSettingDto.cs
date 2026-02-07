using CommunityCar.Domain.Enums.Dashboard.settings;

namespace CommunityCar.Domain.DTOs.Dashboard;

public class SystemSettingDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SettingCategory Category { get; set; }
    public string CategoryText { get; set; } = string.Empty;
    public SettingDataType DataType { get; set; }
    public string DataTypeText { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public bool IsReadOnly { get; set; }
    public string? DefaultValue { get; set; }
    public string? ValidationRegex { get; set; }
    public int DisplayOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class SystemSettingFilterDto
{
    public SettingCategory? Category { get; set; }
    public SettingDataType? DataType { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsReadOnly { get; set; }
    public string? SortBy { get; set; } = "DisplayOrder";
    public bool SortDescending { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class CreateSystemSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SettingCategory Category { get; set; }
    public SettingDataType DataType { get; set; }
    public bool IsEncrypted { get; set; }
    public bool IsReadOnly { get; set; }
    public string? DefaultValue { get; set; }
    public string? ValidationRegex { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateSystemSettingDto
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

public class SettingsSummaryDto
{
    public int TotalSettings { get; set; }
    public int ReadOnlySettings { get; set; }
    public int EncryptedSettings { get; set; }
    public Dictionary<string, int> SettingsByCategory { get; set; } = new();
    public DateTimeOffset LastModified { get; set; }
}
