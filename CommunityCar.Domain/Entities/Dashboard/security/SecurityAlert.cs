using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Dashboard.security;

public class SecurityAlert : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = "Low"; // Low, Medium, High, Critical
    public string Description { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public Guid? ResolvedById { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}
