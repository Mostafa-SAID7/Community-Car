using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Infrastructure.Data;
using CommunityCar.Domain.Entities.Dashboard.security;

namespace CommunityCar.Infrastructure.Services.Common;

public class SecurityService : ISecurityService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SecurityService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
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

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task LogUnauthorizedAccessAsync(string resource, string? metadata = null)
    {
        await LogSecurityEventAsync("UnauthorizedAccess", $"Attempted access to {resource}", metadata);
    }

    public async Task LogSystemAlertAsync(string title, string severity, string description)
    {
        var alert = new SecurityAlert
        {
            Title = title,
            Severity = severity,
            Description = description,
            IsResolved = false
        };

        _context.Set<SecurityAlert>().Add(alert);
        await _context.SaveChangesAsync();
    }
}
