using System.ComponentModel.DataAnnotations;
using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Web.ViewModels.Community.Qa;

public class QuestionViewModel
{
    public Guid Id { get; set; }
    public string? Slug { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Title must be between 10 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [StringLength(5000, MinimumLength = 20, ErrorMessage = "Content must be between 20 and 5000 characters")]
    public string Content { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
    public string? Tags { get; set; }

    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorProfilePicture { get; set; }
    
    public int ViewCount { get; set; }
    public int VoteCount { get; set; }
    public int AnswerCount { get; set; }
    public bool IsResolved { get; set; }
    public Guid? AcceptedAnswerId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class CreateQuestionViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Title must be between 10 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [StringLength(5000, MinimumLength = 20, ErrorMessage = "Content must be between 20 and 5000 characters")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public Guid? CategoryId { get; set; }
    public IEnumerable<CategoryDto>? Categories { get; set; }

    [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
    public string? Tags { get; set; }
}

public class EditQuestionViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Title must be between 10 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [StringLength(5000, MinimumLength = 20, ErrorMessage = "Content must be between 20 and 5000 characters")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public Guid? CategoryId { get; set; }
    public IEnumerable<CategoryDto>? Categories { get; set; }

    [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
    public string? Tags { get; set; }
}
