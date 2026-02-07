using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.ViewModels.Content;

public class SidebarNewsViewModel
{
    public IEnumerable<NewsArticleDto> LatestNews { get; set; } = new List<NewsArticleDto>();
    public IEnumerable<NewsArticleDto> TrendingNews { get; set; } = new List<NewsArticleDto>();
}
