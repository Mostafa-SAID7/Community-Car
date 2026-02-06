using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Domain.Interfaces.Identity;

/// <summary>
/// Service interface for authentication operations
/// </summary>
public interface IAuthenticationService
{
    Task<Result<ApplicationUser>> LoginAsync(string email, string password, bool rememberMe, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
