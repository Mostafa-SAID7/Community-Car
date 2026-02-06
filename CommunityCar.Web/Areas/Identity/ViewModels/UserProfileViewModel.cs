namespace CommunityCar.Web.Areas.Identity.ViewModels;

/// <summary>
/// ViewModel for user profile display
/// </summary>
public class UserProfileViewModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string? Bio { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Slug { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? CreatedAt { get; set; }
}
