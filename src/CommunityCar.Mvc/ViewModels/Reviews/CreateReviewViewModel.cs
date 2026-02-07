using CommunityCar.Domain.Enums.Community.reviews;
using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.ViewModels.Reviews;

public class CreateReviewViewModel
{
    [Required]
    public Guid EntityId { get; set; }

    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    public ReviewType Type { get; set; }

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

    public bool IsVerifiedPurchase { get; set; }

    public bool IsRecommended { get; set; } = true;
}
