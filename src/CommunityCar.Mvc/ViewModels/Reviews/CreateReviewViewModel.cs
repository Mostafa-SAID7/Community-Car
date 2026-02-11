using CommunityCar.Domain.Enums.Community.reviews;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.ViewModels.Reviews;

public class CreateReviewViewModel
{
    // Make EntityId and EntityType optional for standalone reviews
    public Guid? EntityId { get; set; }

    [StringLength(100)]
    public string? EntityType { get; set; }

    [Required]
    public ReviewType Type { get; set; }

    [Required(ErrorMessage = "Rating is required")]
    public decimal Rating { get; set; } = 2.5m; // Default to 2.5 stars

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

    public Guid? GroupId { get; set; }
    
    // Add subject field for standalone reviews
    [StringLength(200)]
    public string? Subject { get; set; }
    
    // Image upload support
    public List<IFormFile>? Images { get; set; }
    
    [StringLength(2000)]
    public string? ImageUrls { get; set; } // JSON array of uploaded image URLs
}
