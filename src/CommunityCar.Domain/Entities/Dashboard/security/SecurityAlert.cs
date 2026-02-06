using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Dashboard.security;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Dashboard.security;

public class SecurityAlert : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public SecuritySeverity Severity { get; private set; }
    public SecurityAlertType AlertType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? Source { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public Guid? AffectedUserId { get; private set; }
    public string? AffectedUserName { get; private set; }
    public bool IsResolved { get; private set; }
    public Guid? ResolvedById { get; private set; }
    public string? ResolvedByName { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public string? ResolutionNotes { get; private set; }
    public DateTimeOffset DetectedAt { get; private set; } = DateTimeOffset.UtcNow;

    private SecurityAlert() { }

    public SecurityAlert(
        string title,
        SecuritySeverity severity,
        SecurityAlertType alertType,
        string description,
        string? source = null,
        string? ipAddress = null,
        string? userAgent = null,
        Guid? affectedUserId = null,
        string? affectedUserName = null)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        Title = title;
        Severity = severity;
        AlertType = alertType;
        Description = description;
        Source = source;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        AffectedUserId = affectedUserId;
        AffectedUserName = affectedUserName;
        IsResolved = false;
        DetectedAt = DateTimeOffset.UtcNow;
    }

    public void Resolve(Guid resolvedById, string resolvedByName, string? resolutionNotes = null)
    {
        Guard.Against.Empty(resolvedById, nameof(resolvedById));
        Guard.Against.NullOrWhiteSpace(resolvedByName, nameof(resolvedByName));

        if (IsResolved)
            throw new InvalidOperationException("Alert is already resolved.");

        IsResolved = true;
        ResolvedById = resolvedById;
        ResolvedByName = resolvedByName;
        ResolvedAt = DateTimeOffset.UtcNow;
        ResolutionNotes = resolutionNotes;
    }

    public void Reopen()
    {
        if (!IsResolved)
            throw new InvalidOperationException("Alert is not resolved.");

        IsResolved = false;
        ResolvedById = null;
        ResolvedByName = null;
        ResolvedAt = null;
        ResolutionNotes = null;
    }

    public void UpdateSeverity(SecuritySeverity newSeverity)
    {
        Severity = newSeverity;
    }

    public void Update(string title, string description, SecuritySeverity severity, SecurityAlertType alertType)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(description, nameof(description));

        Title = title;
        Description = description;
        Severity = severity;
        AlertType = alertType;
    }
}
