using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Web.Areas.Identity.ViewModels;

public class LoginWithRecoveryCodeViewModel
{
    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "Recovery Code")]
    public string RecoveryCode { get; set; } = string.Empty;
}
