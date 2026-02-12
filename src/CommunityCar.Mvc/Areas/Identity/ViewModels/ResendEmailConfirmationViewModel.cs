using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.Areas.Identity.ViewModels;

public class ResendEmailConfirmationViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}
