using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.ViewModels.Reviews;

public class EditReviewViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Review content is required")]
    [StringLength(5000, MinimumLength = 50, ErrorMessage = "Review must be between 50 and 5000 characters")]
    public string Content { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Pros { get; set; }

    [StringLength(1000)]
    public string? Cons { get; set; }

    public bool IsRecommended { get; set; } = true;
}
