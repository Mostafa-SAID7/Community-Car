using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;
using System.Text;

namespace CommunityCar.Mvc.Areas.Dashboard.Controllers.Reports;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ContentReportsController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly IPostService _postService;
    private readonly IEventService _eventService;
    private readonly INewsService _newsService;
    private readonly IGuideService _guideService;
    private readonly IReviewService _reviewService;
    private readonly ILogger<ContentReportsController> _logger;

    public ContentReportsController(
        IQuestionService questionService,
        IPostService postService,
        IEventService eventService,
        INewsService newsService,
        IGuideService guideService,
        IReviewService reviewService,
        ILogger<ContentReportsController> logger)
    {
        _questionService = questionService;
        _postService = postService;
        _eventService = eventService;
        _newsService = newsService;
        _guideService = guideService;
        _reviewService = reviewService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string reportType = "summary", DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var viewModel = new ContentReportsViewModel
            {
                ReportType = reportType,
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-1),
                EndDate = endDate ?? DateTime.UtcNow
            };

            var parameters = new QueryParameters { PageNumber = 1, PageSize = 1000 };

            // Get all content
            var questions = await _questionService.GetQuestionsAsync(parameters);
            var posts = await _postService.GetPostsAsync(parameters);
            var events = await _eventService.GetEventsAsync(parameters);
            var news = await _newsService.GetNewsArticlesAsync(parameters);
            var guides = await _guideService.GetGuidesAsync(parameters);
            var reviews = await _reviewService.GetReviewsAsync(parameters);

            // Filter by date range
            var filteredQuestions = questions.Items.Where(q => q.CreatedAt >= viewModel.StartDate && q.CreatedAt <= viewModel.EndDate).ToList();
            var filteredPosts = posts.Items.Where(p => p.CreatedAt >= viewModel.StartDate && p.CreatedAt <= viewModel.EndDate).ToList();
            var filteredEvents = events.Items.Where(e => e.CreatedAt >= viewModel.StartDate && e.CreatedAt <= viewModel.EndDate).ToList();
            var filteredNews = news.Items.Where(n => n.CreatedAt >= viewModel.StartDate && n.CreatedAt <= viewModel.EndDate).ToList();
            var filteredGuides = guides.Items.Where(g => g.CreatedAt >= viewModel.StartDate && g.CreatedAt <= viewModel.EndDate).ToList();
            var filteredReviews = reviews.Items.Where(r => r.CreatedAt >= viewModel.StartDate && r.CreatedAt <= viewModel.EndDate).ToList();

            // Summary Report
            viewModel.TotalContent = filteredQuestions.Count + filteredPosts.Count + filteredEvents.Count + 
                                     filteredNews.Count + filteredGuides.Count + filteredReviews.Count;
            viewModel.TotalViews = filteredQuestions.Sum(q => q.ViewCount) + filteredPosts.Sum(p => p.ViewCount) + 
                                   filteredNews.Sum(n => n.ViewCount) + filteredGuides.Sum(g => g.ViewCount);
            viewModel.TotalEngagement = filteredQuestions.Sum(q => q.AnswerCount) + filteredPosts.Sum(p => p.CommentCount) + 
                                        filteredNews.Sum(n => n.CommentCount) + filteredGuides.Sum(g => g.CommentCount);

            // Content by type
            viewModel.QuestionCount = filteredQuestions.Count;
            viewModel.PostCount = filteredPosts.Count;
            viewModel.EventCount = filteredEvents.Count;
            viewModel.NewsCount = filteredNews.Count;
            viewModel.GuideCount = filteredGuides.Count;
            viewModel.ReviewCount = filteredReviews.Count;

            // Top authors
            var allContent = new List<(string AuthorName, int ViewCount, int EngagementCount)>();
            allContent.AddRange(filteredQuestions.Select(q => (q.AuthorName, q.ViewCount, q.AnswerCount)));
            allContent.AddRange(filteredPosts.Select(p => (p.AuthorName, p.ViewCount, p.CommentCount)));
            allContent.AddRange(filteredNews.Select(n => (n.AuthorName, n.ViewCount, n.CommentCount)));
            allContent.AddRange(filteredGuides.Select(g => (g.AuthorName, g.ViewCount, g.CommentCount)));

            viewModel.TopAuthors = allContent
                .GroupBy(c => c.AuthorName)
                .Select(g => new ContentReportsViewModel.AuthorStats
                {
                    AuthorName = g.Key,
                    ContentCount = g.Count(),
                    TotalViews = g.Sum(x => x.ViewCount),
                    TotalEngagement = g.Sum(x => x.EngagementCount)
                })
                .OrderByDescending(a => a.ContentCount)
                .Take(10)
                .ToList();

            // Daily breakdown
            var dateRange = Enumerable.Range(0, (viewModel.EndDate - viewModel.StartDate).Days + 1)
                .Select(offset => viewModel.StartDate.AddDays(offset).Date)
                .ToList();

            viewModel.DailyBreakdown = dateRange.Select(date => new ContentReportsViewModel.DailyStats
            {
                Date = date,
                QuestionCount = filteredQuestions.Count(q => q.CreatedAt.Date == date),
                PostCount = filteredPosts.Count(p => p.CreatedAt.Date == date),
                EventCount = filteredEvents.Count(e => e.CreatedAt.Date == date),
                NewsCount = filteredNews.Count(n => n.CreatedAt.Date == date),
                GuideCount = filteredGuides.Count(g => g.CreatedAt.Date == date),
                ReviewCount = filteredReviews.Count(r => r.CreatedAt.Date == date)
            }).ToList();

            // Most popular tags
            var allTags = filteredQuestions.SelectMany(q => (q.Tags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Concat(filteredPosts.SelectMany(p => (p.Tags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)))
                .GroupBy(t => t)
                .Select(g => new ContentReportsViewModel.TagStats
                {
                    TagName = g.Key,
                    UsageCount = g.Count()
                })
                .OrderByDescending(t => t.UsageCount)
                .Take(15)
                .ToList();

            viewModel.PopularTags = allTags;

            // Engagement metrics
            viewModel.AverageViewsPerContent = viewModel.TotalContent > 0 ? (double)viewModel.TotalViews / viewModel.TotalContent : 0;
            viewModel.AverageEngagementPerContent = viewModel.TotalContent > 0 ? (double)viewModel.TotalEngagement / viewModel.TotalContent : 0;

            // Content quality metrics
            viewModel.ResolvedQuestions = filteredQuestions.Count(q => q.IsResolved);
            viewModel.UnresolvedQuestions = filteredQuestions.Count(q => !q.IsResolved);
            viewModel.AverageReviewRating = filteredReviews.Any() ? filteredReviews.Average(r => r.Rating) : 0;

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading content reports");
            TempData["Error"] = "Failed to load reports. Please try again.";
            return View(new ContentReportsViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            var parameters = new QueryParameters { PageNumber = 1, PageSize = 10000 };

            var questions = await _questionService.GetQuestionsAsync(parameters);
            var posts = await _postService.GetPostsAsync(parameters);
            var events = await _eventService.GetEventsAsync(parameters);
            var news = await _newsService.GetNewsArticlesAsync(parameters);
            var guides = await _guideService.GetGuidesAsync(parameters);
            var reviews = await _reviewService.GetReviewsAsync(parameters);

            var csv = new StringBuilder();
            csv.AppendLine("Content Type,Title,Author,Created Date,Views,Engagement,Status");

            // Add questions
            foreach (var q in questions.Items.Where(q => q.CreatedAt >= start && q.CreatedAt <= end))
            {
                csv.AppendLine($"Question,\"{q.Title}\",{q.AuthorName},{q.CreatedAt:yyyy-MM-dd},{q.ViewCount},{q.AnswerCount},{(q.IsResolved ? "Resolved" : "Open")}");
            }

            // Add posts
            foreach (var p in posts.Items.Where(p => p.CreatedAt >= start && p.CreatedAt <= end))
            {
                csv.AppendLine($"Post,\"{p.Title}\",{p.AuthorName},{p.CreatedAt:yyyy-MM-dd},{p.ViewCount},{p.CommentCount},Published");
            }

            // Add events
            foreach (var e in events.Items.Where(e => e.CreatedAt >= start && e.CreatedAt <= end))
            {
                csv.AppendLine($"Event,\"{e.Title}\",{e.OrganizerName},{e.CreatedAt:yyyy-MM-dd},{e.ViewCount},{e.CommentCount},Active");
            }

            // Add news
            foreach (var n in news.Items.Where(n => n.CreatedAt >= start && n.CreatedAt <= end))
            {
                csv.AppendLine($"News,\"{n.Title}\",{n.AuthorName},{n.CreatedAt:yyyy-MM-dd},{n.ViewCount},{n.CommentCount},Published");
            }

            // Add guides
            foreach (var g in guides.Items.Where(g => g.CreatedAt >= start && g.CreatedAt <= end))
            {
                csv.AppendLine($"Guide,\"{g.Title}\",{g.AuthorName},{g.CreatedAt:yyyy-MM-dd},{g.ViewCount},{g.CommentCount},Published");
            }

            // Add reviews
            foreach (var r in reviews.Items.Where(r => r.CreatedAt >= start && r.CreatedAt <= end))
            {
                csv.AppendLine($"Review,\"{r.Title}\",{r.AuthorName},{r.CreatedAt:yyyy-MM-dd},0,0,Rating: {r.Rating}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"content-report-{start:yyyy-MM-dd}-to-{end:yyyy-MM-dd}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting content report");
            TempData["Error"] = "Failed to export report. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportJson(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            var parameters = new QueryParameters { PageNumber = 1, PageSize = 10000 };

            var questions = await _questionService.GetQuestionsAsync(parameters);
            var posts = await _postService.GetPostsAsync(parameters);
            var events = await _eventService.GetEventsAsync(parameters);
            var news = await _newsService.GetNewsArticlesAsync(parameters);
            var guides = await _guideService.GetGuidesAsync(parameters);
            var reviews = await _reviewService.GetReviewsAsync(parameters);

            var report = new
            {
                ReportPeriod = new { Start = start, End = end },
                Summary = new
                {
                    TotalQuestions = questions.Items.Count(q => q.CreatedAt >= start && q.CreatedAt <= end),
                    TotalPosts = posts.Items.Count(p => p.CreatedAt >= start && p.CreatedAt <= end),
                    TotalEvents = events.Items.Count(e => e.CreatedAt >= start && e.CreatedAt <= end),
                    TotalNews = news.Items.Count(n => n.CreatedAt >= start && n.CreatedAt <= end),
                    TotalGuides = guides.Items.Count(g => g.CreatedAt >= start && g.CreatedAt <= end),
                    TotalReviews = reviews.Items.Count(r => r.CreatedAt >= start && r.CreatedAt <= end)
                },
                Questions = questions.Items.Where(q => q.CreatedAt >= start && q.CreatedAt <= end).Select(q => new
                {
                    q.Id,
                    q.Title,
                    q.AuthorName,
                    q.CreatedAt,
                    q.ViewCount,
                    q.AnswerCount,
                    q.IsResolved
                }),
                Posts = posts.Items.Where(p => p.CreatedAt >= start && p.CreatedAt <= end).Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.AuthorName,
                    p.CreatedAt,
                    p.ViewCount,
                    p.CommentCount,
                    p.LikeCount
                }),
                Events = events.Items.Where(e => e.CreatedAt >= start && e.CreatedAt <= end).Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.OrganizerName,
                    e.CreatedAt,
                    e.StartDate,
                    e.Location,
                    e.AttendeeCount
                }),
                News = news.Items.Where(n => n.CreatedAt >= start && n.CreatedAt <= end).Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.AuthorName,
                    n.CreatedAt,
                    n.ViewCount,
                    n.CommentCount
                }),
                Guides = guides.Items.Where(g => g.CreatedAt >= start && g.CreatedAt <= end).Select(g => new
                {
                    g.Id,
                    g.Title,
                    g.AuthorName,
                    g.CreatedAt,
                    g.ViewCount,
                    g.LikeCount
                }),
                Reviews = reviews.Items.Where(r => r.CreatedAt >= start && r.CreatedAt <= end).Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.AuthorName,
                    r.CreatedAt,
                    r.Rating
                })
            };

            return Json(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting JSON report");
            return Json(new { error = "Failed to export report" });
        }
    }
}
