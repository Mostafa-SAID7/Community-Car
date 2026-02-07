using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Dashboard.settings;

public class ApplicationSetting : BaseEntity
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public bool IsEncrypted { get; private set; }
    public DateTimeOffset LastModified { get; private set; }
    public new Guid? ModifiedBy { get; private set; }

    private ApplicationSetting() { }

    public ApplicationSetting(
        string key,
        string value,
        string category,
        string? description = null,
        bool isEncrypted = false)
    {
        Key = key;
        Value = value;
        Category = category;
        Description = description;
        IsEncrypted = isEncrypted;
        LastModified = DateTimeOffset.UtcNow;
    }

    public void UpdateValue(string value, Guid? modifiedBy = null)
    {
        Value = value;
        ModifiedBy = modifiedBy;
        LastModified = DateTimeOffset.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        LastModified = DateTimeOffset.UtcNow;
    }
}
