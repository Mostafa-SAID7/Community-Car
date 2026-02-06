using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Dashboard.AuditLogs;

public class AuditLogEntry : BaseEntity
{
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string? ChangesJson { get; set; }
}
