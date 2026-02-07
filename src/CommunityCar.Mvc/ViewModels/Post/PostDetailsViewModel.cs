using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.ViewModels.Post;

public class PostDetailsViewModel
{
    public PostDto Post { get; set; } = null!;
    public PagedResult<PostCommentDto> Comments { get; set; } = null!;
    public List<PostDto> RelatedPosts { get; set; } = new();
    public string NewComment { get; set; } = string.Empty;
}
