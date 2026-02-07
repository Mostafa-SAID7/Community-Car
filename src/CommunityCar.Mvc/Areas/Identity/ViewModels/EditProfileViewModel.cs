using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Web.Areas.Identity.ViewModels;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
    [Display(Name = "Bio")]
    public string? Bio { get; set; }

    [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
    [Display(Name = "Location")]
    public string? Location { get; set; }

    [Url(ErrorMessage = "Invalid URL")]
    [StringLength(200, ErrorMessage = "Website URL cannot exceed 200 characters")]
    [Display(Name = "Website")]
    public string? Website { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }
}
