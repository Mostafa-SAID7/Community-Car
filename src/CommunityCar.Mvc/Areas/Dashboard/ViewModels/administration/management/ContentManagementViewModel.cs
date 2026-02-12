using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Mvc.Areas.Dashboard.ViewModels.Administration.Management;

public class ContentManagementViewModel
{
    public string ContentType { get; set; } = "all";
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    
    // Content counts for overview
    public int QuestionsCount { get; set; }
    public int PostsCount { get; set; }
    public int EventsCount { get; set; }
    public int NewsCount { get; set; }
    public int GuidesCount { get; set; }
    public int ReviewsCount { get; set; }
    
    // Content lists
    public IEnumerable<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
    public IEnumerable<PostDto> Posts { get; set; } = new List<PostDto>();
    public IEnumerable<EventDto> Events { get; set; } = new List<EventDto>();
    public IEnumerable<NewsArticleDto> News { get; set; } = new List<NewsArticleDto>();
    public IEnumerable<GuideDto> Guides { get; set; } = new List<GuideDto>();
    public IEnumerable<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
}
