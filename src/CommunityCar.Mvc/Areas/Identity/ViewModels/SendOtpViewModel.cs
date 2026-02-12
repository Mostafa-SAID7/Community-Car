using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.Areas.Identity.ViewModels;

public class SendOtpViewModel
{
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
