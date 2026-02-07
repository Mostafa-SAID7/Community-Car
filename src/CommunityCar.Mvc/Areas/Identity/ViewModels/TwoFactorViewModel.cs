using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Web.Areas.Identity.ViewModels;

public class TwoFactorViewModel
{
    [Required(ErrorMessage = "Authenticator code is required")]
    [StringLength(7, MinimumLength = 6, ErrorMessage = "Authenticator code must be 6-7 characters")]
    [Display(Name = "Authenticator Code")]
    public string TwoFactorCode { get; set; } = string.Empty;

    [Display(Name = "Remember this machine")]
    public bool RememberMachine { get; set; }

    public bool RememberMe { get; set; }
}
