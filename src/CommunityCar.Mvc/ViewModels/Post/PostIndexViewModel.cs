using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Enums.Community.post;

namespace CommunityCar.Mvc.ViewModels.Post;

public class PostIndexViewModel
{
    public PagedResult<PostDto> Posts { get; set; } = null!;
    public PostType? FilterType { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
}
