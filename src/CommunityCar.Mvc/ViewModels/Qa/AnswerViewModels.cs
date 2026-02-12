using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.ViewModels.Qa;

public class AnswerViewModel
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }

    [Required(ErrorMessage = "Content is required")]
    [StringLength(5000, MinimumLength = 20, ErrorMessage = "Content must be between 20 and 5000 characters")]
    public string Content { get; set; } = string.Empty;

    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorProfilePicture { get; set; }
    public string AuthorRankName { get; set; } = string.Empty;
    public bool AuthorIsExpert { get; set; }
    
    public int VoteCount { get; set; }
    public bool IsAccepted { get; set; }
    public int CurrentUserVote { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class CreateAnswerViewModel
{
    public Guid QuestionId { get; set; }

    [Required(ErrorMessage = "Content is required")]
    [StringLength(5000, MinimumLength = 20, ErrorMessage = "Content must be between 20 and 5000 characters")]
    [Display(Name = "Your Answer")]
    [DataType(DataType.MultilineText)]
    public string Content { get; set; } = string.Empty;
}

public class EditAnswerViewModel
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }

    [Required(ErrorMessage = "Content is required")]
    [StringLength(5000, MinimumLength = 20, ErrorMessage = "Content must be between 20 and 5000 characters")]
    [Display(Name = "Your Answer")]
    [DataType(DataType.MultilineText)]
    public string Content { get; set; } = string.Empty;
}
