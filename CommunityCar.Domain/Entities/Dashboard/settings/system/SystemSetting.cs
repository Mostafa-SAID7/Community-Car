using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Dashboard.settings.system;

public class SystemSetting : AggregateRoot
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
}
