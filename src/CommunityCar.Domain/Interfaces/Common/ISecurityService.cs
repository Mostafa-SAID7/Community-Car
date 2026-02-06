using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Interfaces.Common;

public interface ISecurityService
{
    Task LogSecurityEventAsync(string action, string description, string? metadata = null);
    Task LogUnauthorizedAccessAsync(string resource, string? metadata = null);
    Task LogSystemAlertAsync(string title, string severity, string description);
}
