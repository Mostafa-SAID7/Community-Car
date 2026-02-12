using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.ViewModels.Support;

public class FAQItemViewModel
{
    public string Category { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

public class ContactUsViewModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Message { get; set; } = string.Empty;
}

public class ReportIssueViewModel
{
    [Required]
    public string IssueType { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public string? Url { get; set; }
}
