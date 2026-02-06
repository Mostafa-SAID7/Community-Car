using CommunityCar.Domain.Base.Interfaces;

namespace CommunityCar.Domain.Commands.Identity;

/// <summary>
/// Command to change user password
/// </summary>
public class ChangePasswordCommand : ICommand
{
    public Guid UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
