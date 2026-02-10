using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.analytics;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Analytics/Content")]
public class ContentAnalyticsController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly IPostService _postService;
    private readonly IEventService _eventService;
    private readonly INewsService _newsService;
    private readonly IGuideService _guideService;
    private readonly IReviewService _reviewService;
    private readonly ILogger<ContentAnalyticsController> _logger;

    public ContentAnalyticsController(
        IQuestionService questionService,
        IPostService postService,
        IEventService eventService,
        INewsService newsService,
        IGuideService guideService,
        IReviewService reviewService,
        ILogger<ContentAnalyticsController> logger)
    {
        _questionService = questionService;
        _postService = postService;
        _eventService = eventService;
        _newsService = newsService;
        _guideService = guideService;
        _reviewService = reviewService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string period = "30days")
    {
        try
        {
            var viewModel = new ContentAnalyticsViewModel
            {
                Period = period
            };

            var parameters = new QueryParameters { PageNumber = 1, PageSize = 100 };

            // Get all content
            var questions = await _questionService.GetQuestionsAsync(parameters);
            var posts = await _postService.GetPostsAsync(parameters);
            var events = await _eventService.GetEventsAsync(parameters);
            var news = await _newsService.GetNewsArticlesAsync(parameters);
            var guides = await _guideService.GetGuidesAsync(parameters);
            var reviews = await _reviewService.GetReviewsAsync(parameters);

            // Calculate date range based on period
            var endDate = DateTime.UtcNow;
            var startDate = period switch
            {
                "7days" => endDate.AddDays(-7),
                "30days" => endDate.AddDays(-30),
                "90days" => endDate.AddDays(-90),
                "1year" => endDate.AddYears(-1),
                _ => endDate.AddDays(-30)
            };

            // Total counts
            viewModel.TotalQuestions = questions.TotalCount;
            viewModel.TotalPosts = posts.TotalCount;
            viewModel.TotalEvents = events.TotalCount;
            viewModel.TotalNews = news.TotalCount;
            viewModel.TotalGuides = guides.TotalCount;
            viewModel.TotalReviews = reviews.TotalCount;

            // New content in period
            viewModel.NewQuestions = questions.Items.Count(q => q.CreatedAt >= startDate);
            viewModel.NewPosts = posts.Items.Count(p => p.CreatedAt >= startDate);
            viewModel.NewEvents = events.Items.Count(e => e.CreatedAt >= startDate);
            viewModel.NewNews = news.Items.Count(n => n.CreatedAt >= startDate);
            viewModel.NewGuides = guides.Items.Count(g => g.CreatedAt >= startDate);
            viewModel.NewReviews = reviews.Items.Count(r => r.CreatedAt >= startDate);

            // Engagement metrics
            viewModel.TotalViews = questions.Items.Sum(q => q.ViewCount) +
                                   posts.Items.Sum(p => p.ViewCount) +
                                   events.Items.Sum(e => e.ViewCount) +
                                   news.Items.Sum(n => n.ViewCount) +
                                   guides.Items.Sum(g => g.ViewCount);

            viewModel.TotalComments = questions.Items.Sum(q => q.AnswerCount) +
                                      posts.Items.Sum(p => p.CommentCount) +
                                      events.Items.Sum(e => e.CommentCount) +
                                      news.Items.Sum(n => n.CommentCount) +
                                      guides.Items.Sum(g => g.CommentCount);

            viewModel.TotalLikes = posts.Items.Sum(p => p.LikeCount) +
                                   guides.Items.Sum(g => g.LikeCount);

            // Most viewed content
            viewModel.MostViewedQuestions = questions.Items
                .OrderByDescending(q => q.ViewCount)
                .Take(5)
                .Select(q => new ContentAnalyticsViewModel.ContentItem
                {
                    Id = q.Id,
                    Title = q.Title,
                    Slug = q.Slug,
                    ViewCount = q.ViewCount,
                    CreatedAt = q.CreatedAt.DateTime,
                    AuthorName = q.AuthorName
                })
                .ToList();

            viewModel.MostViewedPosts = posts.Items
                .OrderByDescending(p => p.ViewCount)
                .Take(5)
                .Select(p => new ContentAnalyticsViewModel.ContentItem
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    ViewCount = p.ViewCount,
                    CreatedAt = p.CreatedAt.DateTime,
                    AuthorName = p.AuthorName
                })
                .ToList();

            // Content growth data (monthly for last 12 months)
            var monthlyData = new Dictionary<string, ContentAnalyticsViewModel.MonthlyContentData>();
            for (int i = 11; i >= 0; i--)
            {
                var monthDate = DateTime.UtcNow.AddMonths(-i);
                var monthKey = monthDate.ToString("MMM yyyy");
                var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                monthlyData[monthKey] = new ContentAnalyticsViewModel.MonthlyContentData
                {
                    Questions = questions.Items.Count(q => q.CreatedAt >= monthStart && q.CreatedAt < monthEnd),
                    Posts = posts.Items.Count(p => p.CreatedAt >= monthStart && p.CreatedAt < monthEnd),
                    Events = events.Items.Count(e => e.CreatedAt >= monthStart && e.CreatedAt < monthEnd),
                    News = news.Items.Count(n => n.CreatedAt >= monthStart && n.CreatedAt < monthEnd),
                    Guides = guides.Items.Count(g => g.CreatedAt >= monthStart && g.CreatedAt < monthEnd),
                    Reviews = reviews.Items.Count(r => r.CreatedAt >= monthStart && r.CreatedAt < monthEnd)
                };
            }
            viewModel.MonthlyGrowth = monthlyData;

            // Content type distribution
            var totalContent = viewModel.TotalQuestions + viewModel.TotalPosts + viewModel.TotalEvents +
                              viewModel.TotalNews + viewModel.TotalGuides + viewModel.TotalReviews;

            if (totalContent > 0)
            {
                viewModel.QuestionsPercentage = (viewModel.TotalQuestions * 100.0) / totalContent;
                viewModel.PostsPercentage = (viewModel.TotalPosts * 100.0) / totalContent;
                viewModel.EventsPercentage = (viewModel.TotalEvents * 100.0) / totalContent;
                viewModel.NewsPercentage = (viewModel.TotalNews * 100.0) / totalContent;
                viewModel.GuidesPercentage = (viewModel.TotalGuides * 100.0) / totalContent;
                viewModel.ReviewsPercentage = (viewModel.TotalReviews * 100.0) / totalContent;
            }

            // Average engagement rates
            if (viewModel.TotalQuestions > 0)
                viewModel.AvgViewsPerQuestion = questions.Items.Average(q => q.ViewCount);
            if (viewModel.TotalPosts > 0)
                viewModel.AvgViewsPerPost = posts.Items.Average(p => p.ViewCount);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading content analytics");
            TempData["Error"] = "Failed to load analytics. Please try again.";
            return View(new ContentAnalyticsViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetContentGrowthData(string period = "30days")
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = 1, PageSize = 1000 };

            var questions = await _questionService.GetQuestionsAsync(parameters);
            var posts = await _postService.GetPostsAsync(parameters);
            var events = await _eventService.GetEventsAsync(parameters);
            var news = await _newsService.GetNewsArticlesAsync(parameters);
            var guides = await _guideService.GetGuidesAsync(parameters);
            var reviews = await _reviewService.GetReviewsAsync(parameters);

            var monthlyData = new Dictionary<string, object>();
            for (int i = 11; i >= 0; i--)
            {
                var monthDate = DateTime.UtcNow.AddMonths(-i);
                var monthKey = monthDate.ToString("MMM yyyy");
                var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                monthlyData[monthKey] = new
                {
                    questions = questions.Items.Count(q => q.CreatedAt >= monthStart && q.CreatedAt < monthEnd),
                    posts = posts.Items.Count(p => p.CreatedAt >= monthStart && p.CreatedAt < monthEnd),
                    events = events.Items.Count(e => e.CreatedAt >= monthStart && e.CreatedAt < monthEnd),
                    news = news.Items.Count(n => n.CreatedAt >= monthStart && n.CreatedAt < monthEnd),
                    guides = guides.Items.Count(g => g.CreatedAt >= monthStart && g.CreatedAt < monthEnd),
                    reviews = reviews.Items.Count(r => r.CreatedAt >= monthStart && r.CreatedAt < monthEnd)
                };
            }

            return Json(monthlyData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting content growth data");
            return Json(new { error = "Failed to load data" });
        }
    }
}
