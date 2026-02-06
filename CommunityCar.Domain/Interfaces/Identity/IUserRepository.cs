using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Interfaces.Identity;

/// <summary>
/// Repository interface for User operations
/// </summary>
public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<PagedResult<ApplicationUser>> GetPagedAsync(QueryParameters parameters, CancellationToken cancellationToken = default);
    Task<ApplicationUser> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
