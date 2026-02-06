using CommunityCar.Domain.Base.Interfaces;
using CommunityCar.Domain.DTOs.Identity;

namespace CommunityCar.Domain.Queries.Identity;

/// <summary>
/// Query to get user by email
/// </summary>
public class GetUserByEmailQuery : IQuery<UserDto>
{
    public string Email { get; set; }

    public GetUserByEmailQuery(string email)
    {
        Email = email;
    }
}
