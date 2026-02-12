using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.Areas.Identity.ViewModels;

public class LoginWithRecoveryCodeViewModel
{
    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "Recovery Code")]
    public string RecoveryCode { get; set; } = string.Empty;
}
