using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Web.Areas.Identity.ViewModels;

public class VerifyOtpViewModel
{
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 digits")]
    [Display(Name = "Verification Code")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
