using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Dashboard.Localization;

public class LocalizationResource : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CultureCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}
