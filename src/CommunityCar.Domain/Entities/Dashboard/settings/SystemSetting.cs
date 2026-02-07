using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Dashboard.settings;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Dashboard.settings;

public class SystemSetting : BaseEntity
{
    private string _key = string.Empty;
    private string _value = string.Empty;

    public string Key
    {
        get => _key;
        private set
        {
            Guard.Against.NullOrWhiteSpace(value, nameof(Key));
            _key = value;
        }
    }

    public string Value
    {
        get => _value;
        private set => _value = value ?? string.Empty;
    }

    public string? Description { get; private set; }
    public SettingCategory Category { get; private set; }
    public SettingDataType DataType { get; private set; }
    public bool IsEncrypted { get; private set; }
    public bool IsReadOnly { get; private set; }
    public string? DefaultValue { get; private set; }
    public string? ValidationRegex { get; private set; }
    public int DisplayOrder { get; private set; }

    private SystemSetting() { } // EF Core

    public SystemSetting(
        string key,
        string value,
        SettingCategory category,
        SettingDataType dataType,
        string? description = null,
        bool isEncrypted = false,
        bool isReadOnly = false,
        string? defaultValue = null,
        string? validationRegex = null,
        int displayOrder = 0)
    {
        Key = key;
        Value = value;
        Category = category;
        DataType = dataType;
        Description = description;
        IsEncrypted = isEncrypted;
        IsReadOnly = isReadOnly;
        DefaultValue = defaultValue;
        ValidationRegex = validationRegex;
        DisplayOrder = displayOrder;
    }

    public void UpdateValue(string newValue)
    {
        Guard.Against.Null(newValue, nameof(newValue));
        
        if (IsReadOnly)
            throw new InvalidOperationException("Cannot update read-only setting");

        Value = newValue;
        ModifiedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        ModifiedAt = DateTimeOffset.UtcNow;
    }

    public void SetReadOnly(bool isReadOnly)
    {
        IsReadOnly = isReadOnly;
        ModifiedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDisplayOrder(int order)
    {
        DisplayOrder = order;
        ModifiedAt = DateTimeOffset.UtcNow;
    }

    public void ResetToDefault()
    {
        if (IsReadOnly)
            throw new InvalidOperationException("Cannot reset read-only setting");

        if (!string.IsNullOrEmpty(DefaultValue))
        {
            Value = DefaultValue;
            ModifiedAt = DateTimeOffset.UtcNow;
        }
    }
}
