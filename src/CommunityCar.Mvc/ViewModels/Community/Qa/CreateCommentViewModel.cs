using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Web.ViewModels.Community.Qa;

public class CreateCommentViewModel
{
    [Required]
    public Guid AnswerId { get; set; }
    
    public Guid QuestionId { get; set; } // For redirect

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string Content { get; set; } = string.Empty;
}
