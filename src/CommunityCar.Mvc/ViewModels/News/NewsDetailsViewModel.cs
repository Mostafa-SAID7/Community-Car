using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.ViewModels.News;

public class NewsDetailsViewModel
{
    public NewsArticleDto Article { get; set; } = null!;
    public PagedResult<NewsCommentDto> Comments { get; set; } = null!;
    public List<NewsArticleDto> RelatedNews { get; set; } = new();
}
