using CommunityCar.Domain.Base.Interfaces;

namespace CommunityCar.Domain.Commands.Identity;

/// <summary>
/// Command to update user profile information
/// </summary>
public class UpdateUserProfileCommand : ICommand
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? PhoneNumber { get; set; }
}
