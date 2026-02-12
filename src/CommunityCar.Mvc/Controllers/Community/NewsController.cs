using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.news;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Mvc.ViewModels.News;
using CommunityCar.Mvc.Controllers.Base;

namespace CommunityCar.Mvc.Controllers.Community;

public class NewsController : BaseController
{
    private readonly INewsService _newsService;
    private readonly IMapper _mapper;
    private readonly ILogger<NewsController> _logger;

    public NewsController(
        INewsService newsService,
        IMapper mapper,
        ILogger<NewsController> logger)
    {
        _newsService = newsService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(
        int page = 1,
        NewsCategory? category = null,
        bool? featured = null)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var parameters = new QueryParameters { PageNumber = page, PageSize = 12 };
            var result = await _newsService.GetNewsArticlesAsync(
                parameters,
                NewsStatus.Published,
                category,
                featured,
                userId);

            ViewBag.CurrentCategory = category;
            ViewBag.Featured = featured;
            ViewBag.Categories = Enum.GetValues(typeof(NewsCategory));

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading news");
            return View(new PagedResult<NewsArticleDto>());
        }
    }

    [HttpGet]
    [AllowAnonymous]
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
            return View(new List<NewsArticleDto>());
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(string slug)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
            var article = await _newsService.GetNewsArticleBySlugAsync(slug, userId);

            if (article == null)
                return NotFound();

            await _newsService.IncrementViewsAsync(article.Id);

            return View(article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading news details");
            return NotFound();
        }
    }

    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        ViewBag.Categories = Enum.GetValues(typeof(NewsCategory));
        return View(new CreateNewsViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateNewsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = Enum.GetValues(typeof(NewsCategory));
            return View(model);
        }

        try
        {
            var userId = GetCurrentUserId();
            var article = await _newsService.CreateNewsArticleAsync(
                model.Title,
                model.Content,
                model.Summary,
                model.Category,
                userId,
                NewsStatus.Published);

            TempData["Success"] = "News article created successfully!";
            return RedirectToAction(nameof(Details), new { slug = article.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating news article");
            ModelState.AddModelError("", "An error occurred while creating the article.");
            ViewBag.Categories = Enum.GetValues(typeof(NewsCategory));
            return View(model);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var article = await _newsService.GetNewsArticleByIdAsync(id, userId);

            if (article == null)
                return NotFound();

            if (article.AuthorId != userId)
                return Forbid();

            var viewModel = _mapper.Map<EditNewsViewModel>(article);
            ViewBag.Categories = Enum.GetValues(typeof(NewsCategory));
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading news for edit");
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditNewsViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = Enum.GetValues(typeof(NewsCategory));
            return View(model);
        }

        try
        {
            var userId = GetCurrentUserId();
            var article = await _newsService.GetNewsArticleByIdAsync(id, userId);

            if (article == null)
                return NotFound();

            if (article.AuthorId != userId)
                return Forbid();

            var updatedArticle = await _newsService.UpdateNewsArticleAsync(
                id,
                model.Title,
                model.Content,
                model.Summary,
                model.Category,
                model.Status);

            TempData["Success"] = "News article updated successfully!";
            return RedirectToAction(nameof(Details), new { slug = updatedArticle.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating news article");
            ModelState.AddModelError("", "An error occurred while updating the article.");
            ViewBag.Categories = Enum.GetValues(typeof(NewsCategory));
            return View(model);
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var article = await _newsService.GetNewsArticleByIdAsync(id, userId);

            if (article == null)
                return NotFound();

            if (article.AuthorId != userId)
                return Forbid();

            await _newsService.DeleteNewsArticleAsync(id);

            TempData["Success"] = "News article deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting news article");
            TempData["Error"] = "An error occurred while deleting the article.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _newsService.ToggleLikeAsync(id, userId);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling like");
            return Json(new { success = false, message = "Failed to toggle like" });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddComment(Guid articleId, string content)
    {
        try
        {
            var userId = GetCurrentUserId();
            var comment = await _newsService.AddCommentAsync(articleId, userId, content);
            return Json(new { success = true, comment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment");
            return Json(new { success = false, message = "Failed to add comment" });
        }
    }
}
