using CommunityCar.Domain.Enums.Community.news;
using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.ViewModels.News;

public class EditNewsViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Summary is required")]
    [StringLength(500, ErrorMessage = "Summary cannot exceed 500 characters")]
    public string Summary { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public NewsCategory Category { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public NewsStatus Status { get; set; }

    public string? ImageUrl { get; set; }
    public string? Source { get; set; }
    public string? ExternalUrl { get; set; }
    public string? Tags { get; set; }
}
