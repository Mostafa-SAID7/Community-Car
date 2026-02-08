using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.post;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.ViewModels.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace CommunityCar.Web.Controllers.Content;

[Route("{culture:alpha}/Posts")]
public class PostsController : Controller
{
    private readonly IPostService _postService;
    private readonly IFriendshipService _friendshipService;
    private readonly ILogger<PostsController> _logger;
    private readonly IStringLocalizer<PostsController> _localizer;

    public PostsController(
        IPostService postService,
        IFriendshipService friendshipService,
        ILogger<PostsController> logger,
        IStringLocalizer<PostsController> localizer)
    {
        _postService = postService;
        _friendshipService = friendshipService;
        _logger = logger;
        _localizer = localizer;
    }

    // GET: Posts
    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 12,
        PostType? type = null,
        Guid? groupId = null)
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var currentUserId = GetCurrentUserId();

            var result = await _postService.GetPostsAsync(
                parameters,
                PostStatus.Published,
                type,
                groupId,
                currentUserId);

            ViewBag.CurrentType = type;
            ViewBag.GroupId = groupId;
            ViewBag.PostTypes = Enum.GetValues<PostType>();

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading posts");
            TempData["Error"] = _localizer["FailedToLoadPosts"];
            return View(new PagedResult<Domain.DTOs.Community.PostDto>(
                new List<Domain.DTOs.Community.PostDto>(), 0, page, pageSize));
        }
    }

    // GET: Posts/Details/{slug}
    [HttpGet("Details/{slug}")]
    [HttpGet("Details")]
    public async Task<IActionResult> Details(string slug)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var postDto = await _postService.GetPostBySlugAsync(slug, currentUserId);

            if (postDto == null)
            {
                TempData["Error"] = _localizer["PostNotFound"];
                return RedirectToAction(nameof(Index));
            }

            // Increment view count
            await _postService.IncrementViewsAsync(postDto.Id);

            var commentsParams = new QueryParameters { PageNumber = 1, PageSize = 10 };
            var comments = await _postService.GetPostCommentsAsync(
                postDto.Id,
                commentsParams,
                currentUserId);

            var relatedPosts = await _postService.GetLatestPostsAsync(5);

            var viewModel = new PostDetailsViewModel
            {
                Post = postDto,
                Comments = comments,
                RelatedPosts = relatedPosts.Where(p => p.Id != postDto.Id).Take(4).ToList()
            };

            if (currentUserId.HasValue)
            {
                var status = await _friendshipService.GetFriendshipStatusAsync(currentUserId.Value, postDto.AuthorId);
                ViewBag.FriendshipStatus = status;
                
                if (status == CommunityCar.Domain.Enums.Community.friends.FriendshipStatus.Pending)
                {
                    var pendingRequests = await _friendshipService.GetPendingRequestsAsync(currentUserId.Value);
                    ViewBag.IsIncomingRequest = pendingRequests.Any(r => r.UserId == postDto.AuthorId);
                }
            }
            else
            {
                ViewBag.FriendshipStatus = CommunityCar.Domain.Enums.Community.friends.FriendshipStatus.None;
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading post {Slug}", slug);
            TempData["Error"] = _localizer["FailedToLoadPost"];
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Post/Create
    [Authorize]
    [HttpGet("Create")]
    public IActionResult Create()
    {
        ViewBag.PostTypes = Enum.GetValues<PostType>();
        return View(new CreatePostViewModel());
    }

    // POST: Post/Create
    [Authorize]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePostViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PostTypes = Enum.GetValues<PostType>();
                return View(model);
            }

            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();

            var post = await _postService.CreatePostAsync(
                model.Title,
                model.Content,
                model.Type,
                userId,
                model.GroupId);

            TempData["Success"] = _localizer["PostCreated"];
            return RedirectToAction(nameof(Details), new { slug = post.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            ModelState.AddModelError("", _localizer["FailedToCreatePost"]);
            ViewBag.PostTypes = Enum.GetValues<PostType>();
            return View(model);
        }
    }

    // GET: Post/Edit/{id}
    [Authorize]
    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            var postDto = await _postService.GetPostByIdAsync(id, userId);

            if (postDto == null)
            {
                TempData["Error"] = _localizer["PostNotFound"];
                return RedirectToAction(nameof(Index));
            }

            if (!postDto.IsAuthor)
            {
                TempData["Error"] = _localizer["OnlyAuthorCanEditPost"];
                return RedirectToAction(nameof(Details), new { slug = postDto.Slug });
            }

            var model = new EditPostViewModel
            {
                Id = postDto.Id,
                Title = postDto.Title,
                Content = postDto.Content,
                Type = postDto.Type,
                ImageUrl = postDto.ImageUrl,
                VideoUrl = postDto.VideoUrl,
                LinkUrl = postDto.LinkUrl,
                LinkTitle = postDto.LinkTitle,
                LinkDescription = postDto.LinkDescription,
                Tags = postDto.Tags
            };

            ViewBag.PostTypes = Enum.GetValues<PostType>();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading post for edit {PostId}", id);
            TempData["Error"] = _localizer["FailedToLoadPost"];
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Post/Edit/{id}
    [Authorize]
    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditPostViewModel model)
    {
        try
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.PostTypes = Enum.GetValues<PostType>();
                return View(model);
            }

            var post = await _postService.UpdatePostAsync(
                id,
                model.Title,
                model.Content,
                model.Type);

            TempData["Success"] = _localizer["PostUpdated"];
            return RedirectToAction(nameof(Details), new { slug = post.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post {PostId}", id);
            ModelState.AddModelError("", _localizer["FailedToUpdatePost"]);
            ViewBag.PostTypes = Enum.GetValues<PostType>();
            return View(model);
        }
    }

    // POST: Post/Delete/{id}
    [Authorize]
    [HttpPost("Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _postService.DeletePostAsync(id);
            return Json(new { success = true, message = _localizer["PostDeleted"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId}", id);
            return Json(new { success = false, message = _localizer["FailedToDeletePost"].Value });
        }
    }

    // POST: Post/Publish/{id}
    [Authorize]
    [HttpPost("Publish/{id:guid}")]
    public async Task<IActionResult> Publish(Guid id)
    {
        try
        {
            await _postService.PublishPostAsync(id);
            return Json(new { success = true, message = _localizer["PostPublished"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing post {PostId}", id);
            return Json(new { success = false, message = _localizer["FailedToPublishPost"].Value });
        }
    }

    // POST: Post/Pin/{id}
    [Authorize]
    [HttpPost("Pin/{id:guid}")]
    public async Task<IActionResult> Pin(Guid id)
    {
        try
        {
            await _postService.PinPostAsync(id);
            return Json(new { success = true, message = _localizer["PostPinned"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pinning post {PostId}", id);
            return Json(new { success = false, message = _localizer["FailedToPinPost"].Value });
        }
    }

    // POST: Post/Lock/{id}
    [Authorize]
    [HttpPost("Lock/{id:guid}")]
    public async Task<IActionResult> Lock(Guid id)
    {
        try
        {
            await _postService.LockPostAsync(id);
            return Json(new { success = true, message = _localizer["PostLocked"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking post {PostId}", id);
            return Json(new { success = false, message = _localizer["FailedToLockPost"].Value });
        }
    }

    // POST: Post/ToggleLike/{id}
    [Authorize]
    [HttpPost("ToggleLike/{id:guid}")]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _postService.ToggleLikeAsync(id, userId);

            return Json(new { success = true, message = _localizer["LikeToggled"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling like for post {PostId}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Post/Share/{id}
    [HttpPost("Share/{id:guid}")]
    public async Task<IActionResult> Share(Guid id)
    {
        try
        {
            await _postService.IncrementSharesAsync(id);
            return Json(new { success = true, message = _localizer["ShareCounted"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing shares for post {PostId}", id);
            return Json(new { success = false, message = _localizer["FailedToCountShare"].Value });
        }
    }

    // POST: Post/AddComment
    [Authorize]
    [HttpPost("AddComment")]
    public async Task<IActionResult> AddComment(Guid postId, string content, Guid? parentCommentId = null)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _postService.AddCommentAsync(postId, userId, content, parentCommentId);

            return Json(new { success = true, message = _localizer["CommentAdded"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to post {PostId}", postId);
            return Json(new { success = false, message = _localizer["FailedToAddComment"].Value });
        }
    }

    // GET: Post/MyPosts
    [Authorize]
    [HttpGet("MyPosts")]
    public async Task<IActionResult> MyPosts(int page = 1, int pageSize = 12)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var result = await _postService.GetUserPostsAsync(userId, parameters, userId);

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user posts");
            TempData["Error"] = _localizer["FailedToLoadMyPosts"];
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Post/Featured
    [HttpGet("Featured")]
    public async Task<IActionResult> Featured()
    {
        try
        {
            var posts = await _postService.GetFeaturedPostsAsync(10);
            return View(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading featured posts");
            TempData["Error"] = _localizer["FailedToLoadFeaturedPosts"];
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
