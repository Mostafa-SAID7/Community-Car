using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.ViewModels.Qa;

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
    public string AuthorRankName { get; set; } = string.Empty;
    public bool AuthorIsExpert { get; set; }
    
    public int ViewCount { get; set; }
    public int VoteCount { get; set; }
    public int AnswerCount { get; set; }
    public int BookmarkCount { get; set; }
    public int ReactionCount { get; set; }
    public int ShareCount { get; set; }
    public bool IsResolved { get; set; }
    public Guid? AcceptedAnswerId { get; set; }
    
    public int CurrentUserVote { get; set; }
    public bool IsBookmarkedByUser { get; set; }
    public int? CurrentUserReactionType { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class QuestionsListViewModel
{
    public List<QuestionViewModel> Questions { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
    public string? Tag { get; set; }
    public string? SortBy { get; set; }
}

public class QuestionDetailsViewModel
{
    public QuestionViewModel Question { get; set; } = new();
    public List<AnswerViewModel> Answers { get; set; } = new();
    public List<QuestionViewModel> RelatedQuestions { get; set; } = new();
}

public class CreateQuestionViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Title must be between 10 and 200 characters")]
    [Display(Name = "Question Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [StringLength(5000, MinimumLength = 20, ErrorMessage = "Content must be between 20 and 5000 characters")]
    [Display(Name = "Question Details")]
    [DataType(DataType.MultilineText)]
    public string Content { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
    [Display(Name = "Tags (comma-separated)")]
    public string? Tags { get; set; }

    [Display(Name = "Category")]
    public Guid? CategoryId { get; set; }

    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? Categories { get; set; }
}

public class EditQuestionViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 10, ErrorMessage = "Title must be between 10 and 200 characters")]
    [Display(Name = "Question Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [StringLength(5000, MinimumLength = 20, ErrorMessage = "Content must be between 20 and 5000 characters")]
    [Display(Name = "Question Details")]
    [DataType(DataType.MultilineText)]
    public string Content { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Tags cannot exceed 500 characters")]
    [Display(Name = "Tags (comma-separated)")]
    public string? Tags { get; set; }

    [Display(Name = "Category")]
    public Guid? CategoryId { get; set; }

    public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? Categories { get; set; }
}
