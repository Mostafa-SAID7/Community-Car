using System.ComponentModel.DataAnnotations;
using CommunityCar.Domain.Enums.Community.post;

namespace CommunityCar.Mvc.ViewModels.Post;

public class CreatePostViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Post type is required")]
    public PostType Type { get; set; } = PostType.Text;

    public Guid? GroupId { get; set; }

    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkTitle { get; set; }
    public string? LinkDescription { get; set; }
    public string? Tags { get; set; }
    public bool PublishImmediately { get; set; }
}
