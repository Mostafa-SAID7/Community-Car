using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.Areas.Identity.ViewModels;

public class VerifyPhoneCodeViewModel
{
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 digits")]
    [Display(Name = "Verification Code")]
    public string Code { get; set; } = string.Empty;
}
