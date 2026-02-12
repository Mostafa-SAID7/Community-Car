using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.ViewModels.Support;

public class ContactFormModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;
}
