using CommunityCar.Domain.Base.Interfaces;
using CommunityCar.Domain.DTOs.Identity;

namespace CommunityCar.Domain.Queries.Identity;

/// <summary>
/// Query to get user by ID
/// </summary>
public class GetUserByIdQuery : IQuery<UserDto>
{
    public Guid UserId { get; set; }

    public GetUserByIdQuery(Guid userId)
    {
        UserId = userId;
    }
}
