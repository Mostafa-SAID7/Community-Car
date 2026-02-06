using CommunityCar.Domain.Base.Interfaces;

namespace CommunityCar.Domain.Commands.Identity;

/// <summary>
/// Command to register a new user
/// </summary>
public class RegisterUserCommand : ICommand
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
