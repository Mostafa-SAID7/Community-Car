using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.Areas.Identity.ViewModels;

public class ProfileSettingsViewModel
{
    [Display(Name = "Email Notifications")]
    public bool EmailNotifications { get; set; }

    [Display(Name = "SMS Notifications")]
    public bool SmsNotifications { get; set; }

    [Display(Name = "Public Profile")]
    public bool PublicProfile { get; set; }

    [Display(Name = "Show Email")]
    public bool ShowEmail { get; set; }

    [Display(Name = "Show Phone Number")]
    public bool ShowPhone { get; set; }
}
