using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Domain.Interfaces.Identity;

/// <summary>
/// Service interface for user operations
/// </summary>
public interface IUserService
{
    Task<Result<ApplicationUser>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<ApplicationUser>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ApplicationUser>>> SearchUsersAsync(QueryParameters parameters, CancellationToken cancellationToken = default);
    Task<Result<Guid>> RegisterUserAsync(string email, string password, string firstName, string lastName, string? phoneNumber, CancellationToken cancellationToken = default);
    Task<Result> UpdateProfileAsync(Guid userId, string firstName, string lastName, string? bio, string? phoneNumber, CancellationToken cancellationToken = default);
}
