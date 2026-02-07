using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.news;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.ViewModels.News;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityCar.Web.Controllers.Content;

[Route("News")]
public class NewsController : Controller
{
    private readonly INewsService _newsService;
    private readonly ILogger<NewsController> _logger;

    public NewsController(
        INewsService newsService,
        ILogger<NewsController> logger)
    {
        _newsService = newsService;
        _logger = logger;
    }

    // GET: News
    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 12,
        NewsCategory? category = null,
        bool? featured = null)
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var currentUserId = GetCurrentUserId();

            var result = await _newsService.GetNewsArticlesAsync(
                parameters,
                NewsStatus.Published,
                category,
                featured,
                currentUserId);

            ViewBag.CurrentCategory = category;
            ViewBag.Featured = featured;
            ViewBag.Categories = Enum.GetValues<NewsCategory>();

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading news articles");
            TempData["Error"] = "Failed to load news articles";
            return View(new PagedResult<Domain.DTOs.Community.NewsArticleDto>(
                new List<Domain.DTOs.Community.NewsArticleDto>(), 0, page, pageSize));
        }
    }

    // GET: News/Details/{slug}
    [HttpGet("Details/{slug}")]
    [HttpGet("Details")]
    public async Task<IActionResult> Details(string slug)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var articleDto = await _newsService.GetNewsArticleBySlugAsync(slug, currentUserId);

            if (articleDto == null)
            {
                TempData["Error"] = "News article not found";
                return RedirectToAction(nameof(Index));
            }

            // Increment view count
            await _newsService.IncrementViewsAsync(articleDto.Id);

            var commentsParams = new QueryParameters { PageNumber = 1, PageSize = 10 };
            var comments = await _newsService.GetNewsCommentsAsync(
                articleDto.Id,
                commentsParams,
                currentUserId);

            var relatedNews = await _newsService.GetLatestNewsAsync(5);

            var viewModel = new NewsDetailsViewModel
            {
                Article = articleDto,
                Comments = comments,
                RelatedNews = relatedNews.Where(n => n.Id != articleDto.Id).Take(4).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading news article {Slug}", slug);
            TempData["Error"] = "Failed to load news article";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: News/Create
    [Authorize]
    [HttpGet("Create")]
    public IActionResult Create()
    {
        ViewBag.Categories = Enum.GetValues<NewsCategory>();
        return View(new CreateNewsViewModel());
    }

    // POST: News/Create
    [Authorize]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateNewsViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = Enum.GetValues<NewsCategory>();
                return View(model);
            }

            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();

            var article = await _newsService.CreateNewsArticleAsync(
                model.Title,
                model.Content,
                model.Summary,
                model.Category,
                userId);

            TempData["Success"] = "News article created successfully";
            return RedirectToAction(nameof(Details), new { slug = article.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating news article");
            ModelState.AddModelError("", "Failed to create news article");
            ViewBag.Categories = Enum.GetValues<NewsCategory>();
            return View(model);
        }
    }

    // GET: News/Edit/{id}
    [Authorize]
    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            var articleDto = await _newsService.GetNewsArticleByIdAsync(id, userId);

            if (articleDto == null)
            {
                TempData["Error"] = "News article not found";
                return RedirectToAction(nameof(Index));
            }

            if (!articleDto.IsAuthor)
            {
                TempData["Error"] = "You can only edit your own articles";
                return RedirectToAction(nameof(Details), new { slug = articleDto.Slug });
            }

            var model = new EditNewsViewModel
            {
                Id = articleDto.Id,
                Title = articleDto.Title,
                Content = articleDto.Content,
                Summary = articleDto.Summary,
                Category = articleDto.Category,
                ImageUrl = articleDto.ImageUrl,
                Source = articleDto.Source,
                ExternalUrl = articleDto.ExternalUrl,
                Tags = articleDto.Tags
            };

            ViewBag.Categories = Enum.GetValues<NewsCategory>();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading news article for edit {ArticleId}", id);
            TempData["Error"] = "Failed to load news article";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: News/Edit/{id}
    [Authorize]
    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditNewsViewModel model)
    {
        try
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = Enum.GetValues<NewsCategory>();
                return View(model);
            }

            var article = await _newsService.UpdateNewsArticleAsync(
                id,
                model.Title,
                model.Content,
                model.Summary,
                model.Category);

            TempData["Success"] = "News article updated successfully";
            return RedirectToAction(nameof(Details), new { slug = article.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating news article {ArticleId}", id);
            ModelState.AddModelError("", "Failed to update news article");
            ViewBag.Categories = Enum.GetValues<NewsCategory>();
            return View(model);
        }
    }

    // POST: News/Delete/{id}
    [Authorize]
    [HttpPost("Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _newsService.DeleteNewsArticleAsync(id);
            return Json(new { success = true, message = "News article deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting news article {ArticleId}", id);
            return Json(new { success = false, message = "Failed to delete news article" });
        }
    }

    // POST: News/Publish/{id}
    [Authorize]
    [HttpPost("Publish/{id:guid}")]
    public async Task<IActionResult> Publish(Guid id)
    {
        try
        {
            await _newsService.PublishNewsArticleAsync(id);
            return Json(new { success = true, message = "News article published successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing news article {ArticleId}", id);
            return Json(new { success = false, message = "Failed to publish news article" });
        }
    }

    // POST: News/Feature/{id}
    [Authorize]
    [HttpPost("Feature/{id:guid}")]
    public async Task<IActionResult> Feature(Guid id)
    {
        try
        {
            await _newsService.FeatureNewsArticleAsync(id);
            return Json(new { success = true, message = "News article featured successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error featuring news article {ArticleId}", id);
            return Json(new { success = false, message = "Failed to feature news article" });
        }
    }

    // POST: News/ToggleLike/{id}
    [Authorize]
    [HttpPost("ToggleLike/{id:guid}")]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _newsService.ToggleLikeAsync(id, userId);

            return Json(new { success = true, message = "Like toggled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling like for news article {ArticleId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: News/Share/{id}
    [HttpPost("Share/{id:guid}")]
    public async Task<IActionResult> Share(Guid id)
    {
        try
        {
            await _newsService.IncrementSharesAsync(id);
            return Json(new { success = true, message = "Share counted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing shares for news article {ArticleId}", id);
            return Json(new { success = false, message = "Failed to count share" });
        }
    }

    // POST: News/AddComment
    [Authorize]
    [HttpPost("AddComment")]
    public async Task<IActionResult> AddComment(Guid articleId, string content)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _newsService.AddCommentAsync(articleId, userId, content);

            return Json(new { success = true, message = "Comment added successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to news article {ArticleId}", articleId);
            return Json(new { success = false, message = "Failed to add comment" });
        }
    }

    // GET: News/MyArticles
    [Authorize]
    [HttpGet("MyArticles")]
    public async Task<IActionResult> MyArticles(int page = 1, int pageSize = 12)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var result = await _newsService.GetUserNewsArticlesAsync(userId, parameters, userId);

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user news articles");
            TempData["Error"] = "Failed to load your articles";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: News/Featured
    [HttpGet("Featured")]
    public async Task<IActionResult> Featured()
    {
        try
        {
            var articles = await _newsService.GetFeaturedNewsAsync(10);
            return View(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading featured news");
            TempData["Error"] = "Failed to load featured news";
            return RedirectToAction(nameof(Index));
        }
    }

    private Guid? GetCurrentUserId()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return null;

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
