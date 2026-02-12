using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Base;
using CommunityCar.Mvc.ViewModels.Post;
using CommunityCar.Mvc.Controllers.Base;
using CommunityCar.Domain.Enums.Community.post;

namespace CommunityCar.Mvc.Controllers.Community;

[Authorize]
public class PostsController : BaseController
{
    private readonly IPostService _postService;
    private readonly IMapper _mapper;
    private readonly ILogger<PostsController> _logger;

    public PostsController(
        IPostService postService,
        IMapper mapper,
        ILogger<PostsController> logger)
    {
        _postService = postService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
        var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
        var result = await _postService.GetPostsAsync(parameters, PostStatus.Published, null, null, userId);
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
        var post = await _postService.GetPostByIdAsync(id, userId);
        if (post == null)
            return NotFound();

        await _postService.IncrementViewsAsync(id);
        return View(post);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = Enum.GetValues(typeof(PostCategory));
        ViewBag.Types = Enum.GetValues(typeof(PostType));
        return View(new CreatePostViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePostViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = Enum.GetValues(typeof(PostCategory));
            ViewBag.Types = Enum.GetValues(typeof(PostType));
            return View(model);
        }

        try
        {
            var userId = GetCurrentUserId();
            var result = await _postService.CreatePostAsync(
                model.Title,
                model.Content,
                model.Type,
                userId,
                model.Category,
                model.GroupId,
                PostStatus.Published);
            
            TempData["Success"] = "Post created successfully!";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            ModelState.AddModelError("", "An error occurred while creating the post.");
            ViewBag.Categories = Enum.GetValues(typeof(PostCategory));
            ViewBag.Types = Enum.GetValues(typeof(PostType));
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var userId = GetCurrentUserId();
        var post = await _postService.GetPostByIdAsync(id, userId);
        if (post == null)
            return NotFound();

        if (post.AuthorId != userId)
            return Forbid();

        var viewModel = _mapper.Map<EditPostViewModel>(post);
        ViewBag.Categories = Enum.GetValues(typeof(PostCategory));
        ViewBag.Types = Enum.GetValues(typeof(PostType));
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditPostViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = Enum.GetValues(typeof(PostCategory));
            ViewBag.Types = Enum.GetValues(typeof(PostType));
            return View(model);
        }

        try
        {
            var userId = GetCurrentUserId();
            var post = await _postService.GetPostByIdAsync(id, userId);
            if (post == null)
                return NotFound();

            if (post.AuthorId != userId)
                return Forbid();

            await _postService.UpdatePostAsync(
                id,
                model.Title,
                model.Content,
                model.Type,
                model.Category,
                null);
            
            TempData["Success"] = "Post updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post {PostId}", id);
            ModelState.AddModelError("", "An error occurred while updating the post.");
            ViewBag.Categories = Enum.GetValues(typeof(PostCategory));
            ViewBag.Types = Enum.GetValues(typeof(PostType));
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var post = await _postService.GetPostByIdAsync(id, userId);
            if (post == null)
                return NotFound();

            if (post.AuthorId != userId)
                return Forbid();

            await _postService.DeletePostAsync(id);
            
            TempData["Success"] = "Post deleted successfully!";
            return RedirectToAction(nameof(MyPosts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId}", id);
            TempData["Error"] = "An error occurred while deleting the post.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyPosts(int page = 1, int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
        var result = await _postService.GetUserPostsAsync(userId, parameters, userId);
        return View(result);
    }

    [HttpPost]
    public async Task<IActionResult> Like(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _postService.ToggleLikeAsync(id, userId);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking post {PostId}", id);
            return Json(new { success = false, message = "Failed to like post" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Unlike(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _postService.ToggleLikeAsync(id, userId);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unliking post {PostId}", id);
            return Json(new { success = false, message = "Failed to unlike post" });
        }
    }
}
