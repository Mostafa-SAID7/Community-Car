using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.Areas.Identity.ViewModels;

public class LoginWithPhoneViewModel
{
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "OTP code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
    [Display(Name = "OTP Code")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }
}
