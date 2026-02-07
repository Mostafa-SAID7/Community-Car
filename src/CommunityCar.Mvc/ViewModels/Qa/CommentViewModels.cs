using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Mvc.ViewModels.Qa;

public class CreateCommentViewModel
{
    public Guid QuestionId { get; set; }
    
    [Required]
    public Guid AnswerId { get; set; }
    
    [Required(ErrorMessage = "Comment content is required")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 1000 characters")]
    public string Content { get; set; } = string.Empty;
}

public class EditCommentViewModel
{
    public Guid Id { get; set; }
    public Guid AnswerId { get; set; }
    
    [Required(ErrorMessage = "Comment content is required")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 1000 characters")]
    public string Content { get; set; } = string.Empty;
}

public class ReportCommentViewModel
{
    public Guid CommentId { get; set; }
    
    [Required(ErrorMessage = "Please provide a reason for reporting")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 500 characters")]
    public string Reason { get; set; } = string.Empty;
}
