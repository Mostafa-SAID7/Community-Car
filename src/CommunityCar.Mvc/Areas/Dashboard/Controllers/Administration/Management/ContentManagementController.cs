using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.Areas.Dashboard.ViewModels;

namespace CommunityCar.Web.Areas.Dashboard.Controllers.management;

[Area("Dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("{culture}/Dashboard/Administration/Management/Content")]
public class ContentManagementController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly IPostService _postService;
    private readonly IEventService _eventService;
    private readonly INewsService _newsService;
    private readonly IGuideService _guideService;
    private readonly IReviewService _reviewService;
    private readonly ILogger<ContentManagementController> _logger;

    public ContentManagementController(
        IQuestionService questionService,
        IPostService postService,
        IEventService eventService,
        INewsService newsService,
        IGuideService guideService,
        IReviewService reviewService,
        ILogger<ContentManagementController> logger)
    {
        _questionService = questionService;
        _postService = postService;
        _eventService = eventService;
        _newsService = newsService;
        _guideService = guideService;
        _reviewService = reviewService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string contentType = "all", int page = 1, int pageSize = 20)
    {
        try
        {
            var viewModel = new ContentManagementViewModel
            {
                ContentType = contentType,
                CurrentPage = page,
                PageSize = pageSize
            };

            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };

            switch (contentType.ToLower())
            {
                case "questions":
                    var questions = await _questionService.GetQuestionsAsync(parameters);
                    viewModel.Questions = questions.Items;
                    viewModel.TotalCount = questions.TotalCount;
                    viewModel.TotalPages = questions.TotalPages;
                    break;

                case "posts":
                    var posts = await _postService.GetPostsAsync(parameters);
                    viewModel.Posts = posts.Items;
                    viewModel.TotalCount = posts.TotalCount;
                    viewModel.TotalPages = posts.TotalPages;
                    break;

                case "events":
                    var events = await _eventService.GetEventsAsync(parameters);
                    viewModel.Events = events.Items;
                    viewModel.TotalCount = events.TotalCount;
                    viewModel.TotalPages = events.TotalPages;
                    break;

                case "news":
                    var news = await _newsService.GetNewsArticlesAsync(parameters);
                    viewModel.News = news.Items;
                    viewModel.TotalCount = news.TotalCount;
                    viewModel.TotalPages = news.TotalPages;
                    break;

                case "guides":
                    var guides = await _guideService.GetGuidesAsync(parameters);
                    viewModel.Guides = guides.Items;
                    viewModel.TotalCount = guides.TotalCount;
                    viewModel.TotalPages = guides.TotalPages;
                    break;

                case "reviews":
                    var reviews = await _reviewService.GetReviewsAsync(parameters);
                    viewModel.Reviews = reviews.Items;
                    viewModel.TotalCount = reviews.TotalCount;
                    viewModel.TotalPages = reviews.TotalPages;
                    break;

                default:
                    // Get summary counts for all content types
                    var questionsCount = await _questionService.GetQuestionsAsync(new QueryParameters { PageNumber = 1, PageSize = 1 });
                    var postsCount = await _postService.GetPostsAsync(new QueryParameters { PageNumber = 1, PageSize = 1 });
                    var eventsCount = await _eventService.GetEventsAsync(new QueryParameters { PageNumber = 1, PageSize = 1 });
                    var newsCount = await _newsService.GetNewsArticlesAsync(new QueryParameters { PageNumber = 1, PageSize = 1 });
                    var guidesCount = await _guideService.GetGuidesAsync(new QueryParameters { PageNumber = 1, PageSize = 1 });
                    var reviewsCount = await _reviewService.GetReviewsAsync(new QueryParameters { PageNumber = 1, PageSize = 1 });

                    viewModel.QuestionsCount = questionsCount.TotalCount;
                    viewModel.PostsCount = postsCount.TotalCount;
                    viewModel.EventsCount = eventsCount.TotalCount;
                    viewModel.NewsCount = newsCount.TotalCount;
                    viewModel.GuidesCount = guidesCount.TotalCount;
                    viewModel.ReviewsCount = reviewsCount.TotalCount;
                    break;
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading content management");
            TempData["Error"] = "Failed to load content. Please try again.";
            return View(new ContentManagementViewModel());
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        try
        {
            await _questionService.DeleteQuestionAsync(id);
            TempData["Success"] = "Question deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting question {QuestionId}", id);
            TempData["Error"] = "An error occurred while deleting the question.";
        }

        return RedirectToAction(nameof(Index), new { contentType = "questions" });
    }

    [HttpPost]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        try
        {
            await _postService.DeletePostAsync(id);
            TempData["Success"] = "Post deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId}", id);
            TempData["Error"] = "An error occurred while deleting the post.";
        }

        return RedirectToAction(nameof(Index), new { contentType = "posts" });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        try
        {
            await _eventService.DeleteEventAsync(id);
            TempData["Success"] = "Event deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", id);
            TempData["Error"] = "An error occurred while deleting the event.";
        }

        return RedirectToAction(nameof(Index), new { contentType = "events" });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteNews(Guid id)
    {
        try
        {
            await _newsService.DeleteNewsArticleAsync(id);
            TempData["Success"] = "News article deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting news {NewsId}", id);
            TempData["Error"] = "An error occurred while deleting the news article.";
        }

        return RedirectToAction(nameof(Index), new { contentType = "news" });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteGuide(Guid id)
    {
        try
        {
            await _guideService.DeleteGuideAsync(id);
            TempData["Success"] = "Guide deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting guide {GuideId}", id);
            TempData["Error"] = "An error occurred while deleting the guide.";
        }

        return RedirectToAction(nameof(Index), new { contentType = "guides" });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        try
        {
            await _reviewService.DeleteReviewAsync(id);
            TempData["Success"] = "Review deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", id);
            TempData["Error"] = "An error occurred while deleting the review.";
        }

        return RedirectToAction(nameof(Index), new { contentType = "reviews" });
    }
}
