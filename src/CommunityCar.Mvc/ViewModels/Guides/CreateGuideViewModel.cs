using CommunityCar.Domain.Enums.Community.guides;
using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.ViewModels.Guides;

public class CreateGuideViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Summary is required")]
    [StringLength(500, ErrorMessage = "Summary cannot exceed 500 characters")]
    public string Summary { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Difficulty is required")]
    public GuideDifficulty Difficulty { get; set; }

    [Required(ErrorMessage = "Estimated time is required")]
    [Range(1, 1440, ErrorMessage = "Estimated time must be between 1 and 1440 minutes")]
    public int EstimatedTimeMinutes { get; set; }

    public string? Tags { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
}
