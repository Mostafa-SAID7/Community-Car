using CommunityCar.Domain.Base.Interfaces;

namespace CommunityCar.Domain.Commands.Identity;

/// <summary>
/// Command to authenticate a user
/// </summary>
public class LoginCommand : ICommand
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
