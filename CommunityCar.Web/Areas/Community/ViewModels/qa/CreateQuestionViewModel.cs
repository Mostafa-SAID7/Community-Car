using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Web.Areas.Community.ViewModels.qa;

public class CreateQuestionViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [StringLength(10000, ErrorMessage = "Content cannot exceed 10000 characters")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Category")]
    public Guid? CategoryId { get; set; }

    [Display(Name = "Tags")]
    public string? Tags { get; set; }
}
