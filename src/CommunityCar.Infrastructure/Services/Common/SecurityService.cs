using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Entities.Dashboard.security;
using CommunityCar.Domain.Enums.Dashboard.security;

namespace CommunityCar.Infrastructure.Services.Common;

public class SecurityService : ISecurityService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;

    public SecurityService(IUnitOfWork uow, ICurrentUserService currentUserService)
    {
        _uow = uow;
        _currentUserService = currentUserService;
    }

    public async Task LogSecurityEventAsync(string action, string description, string? metadata = null)
    {
        var auditLog = new AuditLog
        {
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.UserName ?? "System",
            Action = $"SecurityEvent:{action}",
            EntityName = "Security",
            Description = description,
            NewValues = metadata
        };

        await _uow.Repository<AuditLog>().AddAsync(auditLog);
        await _uow.SaveChangesAsync();
    }

    public async Task LogUnauthorizedAccessAsync(string resource, string? metadata = null)
    {
        await LogSecurityEventAsync("UnauthorizedAccess", $"Attempted access to {resource}", metadata);
    }

    public async Task LogSystemAlertAsync(string title, string severity, string description)
    {
        // Parse severity string to enum
        var severityEnum = Enum.TryParse<SecuritySeverity>(severity, true, out var parsedSeverity) 
            ? parsedSeverity 
            : SecuritySeverity.Low;

        var alert = new SecurityAlert(
            title,
            severityEnum,
            SecurityAlertType.Other,
            description,
            source: "System");

        await _uow.Repository<SecurityAlert>().AddAsync(alert);
        await _uow.SaveChangesAsync();
    }
}
