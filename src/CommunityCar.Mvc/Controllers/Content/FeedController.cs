using CommunityCar.Domain.Base;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.ViewModels.Feed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Content;

[Route("{culture:alpha}/[controller]")]
[Route("[controller]")]
public class FeedController : Controller
{
    private readonly IPostService _postService;
    private readonly IQuestionService _questionService;
    private readonly IEventService _eventService;
    private readonly INewsService _newsService;
    private readonly IGuideService _guideService;
    private readonly IReviewService _reviewService;
    private readonly IGroupService _groupService;
    private readonly ILogger<FeedController> _logger;
    private readonly IStringLocalizer<FeedController> _localizer;

    public FeedController(
        IPostService postService,
        IQuestionService questionService,
        IEventService eventService,
        INewsService newsService,
        IGuideService guideService,
        IReviewService reviewService,
        IGroupService groupService,
        ILogger<FeedController> logger,
        IStringLocalizer<FeedController> localizer)
    {
        _postService = postService;
        _questionService = questionService;
        _eventService = eventService;
        _newsService = newsService;
        _guideService = guideService;
        _reviewService = reviewService;
        _groupService = groupService;
        _logger = logger;
        _localizer = localizer;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return null;
        }
        return userId;
    }

    [HttpGet("/")]
    [HttpGet("/{culture:alpha}")]
    [HttpGet("")]
    [AllowAnonymous]
    public async Task<IActionResult> Index(
        FeedItemType? type = null,
        string? search = null,
        string? category = null,
        DateFilterType dateFilter = DateFilterType.All,
        FeedSortType sortBy = FeedSortType.Recent,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            
            var feedItems = new List<FeedItemViewModel>();

            // Fetch content based on type filter
            if (type == null || type == FeedItemType.Post)
            {
                var posts = await _postService.GetPostsAsync(parameters, currentUserId: userId);
                feedItems.AddRange(posts.Items.Select(MapPostToFeedItem));
            }

            if (type == null || type == FeedItemType.Question)
            {
                var questions = await _questionService.GetQuestionsAsync(parameters, searchTerm: search, currentUserId: userId);
                feedItems.AddRange(questions.Items.Select(MapQuestionToFeedItem));
            }

            if (type == null || type == FeedItemType.Event)
            {
                var events = await _eventService.GetEventsAsync(parameters, currentUserId: userId);
                feedItems.AddRange(events.Items.Select(MapEventToFeedItem));
            }

            if (type == null || type == FeedItemType.News)
            {
                var news = await _newsService.GetNewsArticlesAsync(parameters, currentUserId: userId);
                feedItems.AddRange(news.Items.Select(MapNewsToFeedItem));
            }

            if (type == null || type == FeedItemType.Guide)
            {
                var guides = await _guideService.GetGuidesAsync(parameters, currentUserId: userId);
                feedItems.AddRange(guides.Items.Select(MapGuideToFeedItem));
            }

            if (type == null || type == FeedItemType.Review)
            {
                var reviews = await _reviewService.GetReviewsAsync(parameters, currentUserId: userId);
                feedItems.AddRange(reviews.Items.Select(MapReviewToFeedItem));
            }

            if (type == null || type == FeedItemType.Group)
            {
                var groups = await _groupService.GetGroupsAsync(parameters, currentUserId: userId);
                feedItems.AddRange(groups.Items.Select(MapGroupToFeedItem));
            }

            // Apply date filter
            feedItems = ApplyDateFilter(feedItems, dateFilter);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                feedItems = feedItems.Where(f => 
                    f.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    f.Content.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Apply sorting
            feedItems = ApplySorting(feedItems, sortBy);

            // Paginate
            var totalCount = feedItems.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            feedItems = feedItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new FeedViewModel
            {
                Items = feedItems,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                Filters = new FeedFilterViewModel
                {
                    ContentType = type,
                    SearchTerm = search,
                    Category = category,
                    DateFilter = dateFilter,
                    SortBy = sortBy
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading feed");
            TempData["Error"] = _localizer["FailedToLoadFeed"];
            return View(new FeedViewModel());
        }
    }

    [HttpGet("Trending")]
    [AllowAnonymous]
    public async Task<IActionResult> Trending(int page = 1, int pageSize = 20)
    {
        return await Index(sortBy: FeedSortType.Trending, page: page, pageSize: pageSize);
    }

    [HttpGet("Popular")]
    [AllowAnonymous]
    public async Task<IActionResult> Popular(int page = 1, int pageSize = 20)
    {
        return await Index(sortBy: FeedSortType.Popular, page: page, pageSize: pageSize);
    }

    private FeedItemViewModel MapPostToFeedItem(Domain.DTOs.Community.PostDto post)
    {
        return new FeedItemViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = TruncateContent(post.Content, 200),
            Slug = post.Slug,
            Type = FeedItemType.Post,
            TypeName = _localizer["Post"].Value,
            TypeIcon = "fa-file-text",
            TypeColor = "primary",
            AuthorId = post.AuthorId,
            AuthorName = post.AuthorName,
            AuthorAvatar = post.AuthorAvatar,
            ImageUrl = post.ImageUrl,
            Tags = post.Tags,
            ViewCount = post.ViewCount,
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            InteractionCount = post.LikeCount + post.CommentCount + post.ShareCount,
            CreatedAt = post.CreatedAt,
            TimeAgo = GetTimeAgo(post.CreatedAt),
            ActionUrl = $"/Posts/Details/{post.Slug}",
            ActionText = _localizer["ReadMore"].Value
        };
    }

    private FeedItemViewModel MapQuestionToFeedItem(Domain.DTOs.Community.QuestionDto question)
    {
        return new FeedItemViewModel
        {
            Id = question.Id,
            Title = question.Title,
            Content = TruncateContent(question.Content, 200),
            Slug = question.Slug ?? string.Empty,
            Type = FeedItemType.Question,
            TypeName = _localizer["Question"].Value,
            TypeIcon = "fa-question-circle",
            TypeColor = "info",
            AuthorId = question.AuthorId,
            AuthorName = question.AuthorName,
            AuthorAvatar = question.AuthorProfilePicture,
            Category = question.Category?.Name,
            Tags = question.Tags,
            ViewCount = question.ViewCount,
            LikeCount = question.VoteCount,
            CommentCount = question.AnswerCount,
            InteractionCount = question.VoteCount + question.AnswerCount,
            IsResolved = question.IsResolved,
            AnswerCount = question.AnswerCount,
            CreatedAt = question.CreatedAt,
            TimeAgo = GetTimeAgo(question.CreatedAt),
            ActionUrl = $"/Questions/Details/{question.Slug}",
            ActionText = _localizer["ViewQuestion"].Value
        };
    }

    private FeedItemViewModel MapEventToFeedItem(Domain.DTOs.Community.EventDto evt)
    {
        return new FeedItemViewModel
        {
            Id = evt.Id,
            Title = evt.Title,
            Content = TruncateContent(evt.Description, 200),
            Slug = evt.Slug,
            Type = FeedItemType.Event,
            TypeName = _localizer["Event"].Value,
            TypeIcon = "fa-calendar",
            TypeColor = "success",
            AuthorId = evt.OrganizerId,
            AuthorName = evt.OrganizerName,
            AuthorAvatar = evt.OrganizerAvatar,
            ImageUrl = evt.ImageUrl,
            Category = evt.CategoryName,
            ViewCount = 0,
            LikeCount = evt.InterestedCount,
            CommentCount = 0,
            InteractionCount = evt.AttendeeCount + evt.InterestedCount,
            EventStartTime = evt.StartTime,
            EventLocation = evt.Location,
            CreatedAt = evt.CreatedAt,
            TimeAgo = GetTimeAgo(evt.CreatedAt),
            ActionUrl = $"/Events/Details/{evt.Slug}",
            ActionText = _localizer["ViewEvent"].Value
        };
    }

    private FeedItemViewModel MapNewsToFeedItem(Domain.DTOs.Community.NewsArticleDto news)
    {
        return new FeedItemViewModel
        {
            Id = news.Id,
            Title = news.Title,
            Content = TruncateContent(news.Summary, 200),
            Slug = news.Slug,
            Type = FeedItemType.News,
            TypeName = _localizer["News"].Value,
            TypeIcon = "fa-newspaper-o",
            TypeColor = "warning",
            AuthorId = news.AuthorId,
            AuthorName = news.AuthorName,
            AuthorAvatar = news.AuthorAvatar,
            ImageUrl = news.ImageUrl,
            Category = news.CategoryName,
            Tags = news.Tags,
            ViewCount = news.ViewCount,
            LikeCount = news.LikeCount,
            CommentCount = news.CommentCount,
            InteractionCount = news.LikeCount + news.CommentCount + news.ShareCount,
            CreatedAt = news.CreatedAt,
            TimeAgo = GetTimeAgo(news.CreatedAt),
            ActionUrl = $"/News/Details/{news.Slug}",
            ActionText = _localizer["ReadArticle"].Value
        };
    }

    private FeedItemViewModel MapGuideToFeedItem(Domain.DTOs.Community.GuideDto guide)
    {
        return new FeedItemViewModel
        {
            Id = guide.Id,
            Title = guide.Title,
            Content = TruncateContent(guide.Summary, 200),
            Slug = guide.Slug,
            Type = FeedItemType.Guide,
            TypeName = _localizer["Guide"].Value,
            TypeIcon = "fa-book",
            TypeColor = "purple",
            AuthorId = guide.AuthorId,
            AuthorName = guide.AuthorName,
            AuthorAvatar = guide.AuthorAvatar,
            ImageUrl = guide.ImageUrl,
            Category = guide.Category,
            Tags = guide.Tags,
            ViewCount = guide.ViewCount,
            LikeCount = guide.LikeCount,
            CommentCount = 0,
            InteractionCount = guide.LikeCount + guide.BookmarkCount,
            CreatedAt = guide.CreatedAt,
            TimeAgo = GetTimeAgo(guide.CreatedAt),
            ActionUrl = $"/Guides/Details/{guide.Slug}",
            ActionText = _localizer["ReadGuide"].Value
        };
    }

    private FeedItemViewModel MapReviewToFeedItem(Domain.DTOs.Community.ReviewDto review)
    {
        return new FeedItemViewModel
        {
            Id = review.Id,
            Title = review.Title,
            Content = TruncateContent(review.Content, 200),
            Slug = review.Slug,
            Type = FeedItemType.Review,
            TypeName = _localizer["Review"].Value,
            TypeIcon = "fa-star",
            TypeColor = "danger",
            AuthorId = review.ReviewerId,
            AuthorName = review.ReviewerName,
            AuthorAvatar = review.ReviewerAvatar,
            Category = review.TypeName,
            ViewCount = 0,
            LikeCount = review.HelpfulCount,
            CommentCount = 0,
            InteractionCount = review.HelpfulCount + review.NotHelpfulCount,
            Rating = review.Rating,
            IsRecommended = review.IsRecommended,
            CreatedAt = review.CreatedAt,
            TimeAgo = GetTimeAgo(review.CreatedAt),
            ActionUrl = $"/Reviews/Details/{review.Slug}",
            ActionText = _localizer["ReadReview"].Value
        };
    }

    private FeedItemViewModel MapGroupToFeedItem(Domain.DTOs.Community.GroupDto group)
    {
        return new FeedItemViewModel
        {
            Id = group.Id,
            Title = group.Name,
            Content = TruncateContent(group.Description, 200),
            Slug = group.Slug ?? string.Empty,
            Type = FeedItemType.Group,
            TypeName = _localizer["Group"].Value,
            TypeIcon = "fa-users",
            TypeColor = "info",
            AuthorId = group.CreatorId,
            AuthorName = group.CreatorName,
            AuthorAvatar = null,
            ImageUrl = group.ImageUrl,
            ViewCount = 0,
            LikeCount = 0,
            CommentCount = 0,
            InteractionCount = group.MemberCount,
            MemberCount = group.MemberCount,
            IsPrivate = group.IsPrivate,
            IsUserMember = group.IsUserMember,
            CreatedAt = group.CreatedAt,
            TimeAgo = GetTimeAgo(group.CreatedAt),
            ActionUrl = $"/Groups/Details/{group.Slug}",
            ActionText = _localizer["ViewGroup"].Value
        };
    }

    private List<FeedItemViewModel> ApplyDateFilter(List<FeedItemViewModel> items, DateFilterType filter)
    {
        var now = DateTimeOffset.UtcNow;
        return filter switch
        {
            DateFilterType.Today => items.Where(i => i.CreatedAt.Date == now.Date).ToList(),
            DateFilterType.ThisWeek => items.Where(i => i.CreatedAt >= now.AddDays(-7)).ToList(),
            DateFilterType.ThisMonth => items.Where(i => i.CreatedAt >= now.AddDays(-30)).ToList(),
            _ => items
        };
    }

    private List<FeedItemViewModel> ApplySorting(List<FeedItemViewModel> items, FeedSortType sortBy)
    {
        return sortBy switch
        {
            FeedSortType.Popular => items.OrderByDescending(i => i.InteractionCount).ThenByDescending(i => i.CreatedAt).ToList(),
            FeedSortType.Trending => items.OrderByDescending(i => CalculateTrendingScore(i)).ToList(),
            _ => items.OrderByDescending(i => i.CreatedAt).ToList()
        };
    }

    private double CalculateTrendingScore(FeedItemViewModel item)
    {
        var hoursSinceCreation = (DateTimeOffset.UtcNow - item.CreatedAt).TotalHours;
        if (hoursSinceCreation < 1) hoursSinceCreation = 1;
        
        return item.InteractionCount / Math.Pow(hoursSinceCreation, 1.5);
    }

    private string TruncateContent(string content, int maxLength)
    {
        if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
            return content;
        
        return content.Substring(0, maxLength) + "...";
    }

    private string GetTimeAgo(DateTimeOffset date)
    {
        var timeSpan = DateTimeOffset.UtcNow - date;
        
        if (timeSpan.TotalMinutes < 1) return _localizer["JustNow"].Value;
        if (timeSpan.TotalMinutes < 60) return string.Format(_localizer["MinutesAgo"].Value, (int)timeSpan.TotalMinutes);
        if (timeSpan.TotalHours < 24) return string.Format(_localizer["HoursAgo"].Value, (int)timeSpan.TotalHours);
        if (timeSpan.TotalDays < 7) return string.Format(_localizer["DaysAgo"].Value, (int)timeSpan.TotalDays);
        if (timeSpan.TotalDays < 30) return string.Format(_localizer["WeeksAgo"].Value, (int)(timeSpan.TotalDays / 7));
        if (timeSpan.TotalDays < 365) return string.Format(_localizer["MonthsAgo"].Value, (int)(timeSpan.TotalDays / 30));
        
        return string.Format(_localizer["YearsAgo"].Value, (int)(timeSpan.TotalDays / 365));
    }
}
