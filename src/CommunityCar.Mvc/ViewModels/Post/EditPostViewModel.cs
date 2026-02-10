using System.ComponentModel.DataAnnotations;
using CommunityCar.Domain.Enums.Community.post;
using Microsoft.AspNetCore.Http;

namespace CommunityCar.Mvc.ViewModels.Post;

public class EditPostViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Post type is required")]
    public PostType Type { get; set; }

    public PostCategory Category { get; set; } = PostCategory.General;

    [Required(ErrorMessage = "Status is required")]
    public PostStatus Status { get; set; } = PostStatus.Draft;

    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkTitle { get; set; }
    public string? LinkDescription { get; set; }
    public string? Tags { get; set; }

    // File upload properties
    public IFormFile? ImageFile { get; set; }
    public IFormFile? VideoFile { get; set; }
}
