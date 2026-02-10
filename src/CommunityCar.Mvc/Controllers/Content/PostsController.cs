using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.post;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Infrastructure.Interfaces;
using CommunityCar.Infrastructure.Services.Community;
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
    private readonly IGroupService _groupService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IPostHubService _postHubService;
    private readonly ILogger<PostsController> _logger;
    private readonly IStringLocalizer<PostsController> _localizer;

    public PostsController(
        IPostService postService,
        IFriendshipService friendshipService,
        IGroupService groupService,
        IFileStorageService fileStorageService,
        IPostHubService postHubService,
        ILogger<PostsController> logger,
        IStringLocalizer<PostsController> localizer)
    {
        _postService = postService;
        _friendshipService = friendshipService;
        _groupService = groupService;
        _fileStorageService = fileStorageService;
        _postHubService = postHubService;
        _logger = logger;
        _localizer = localizer;
    }

    // GET: Posts
    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 12,
        PostType? type = null,
        Guid? groupId = null,
        string? search = null)
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

            // Apply client-side search filter if search term provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                var filteredItems = result.Items.Where(p =>
                    (p.Title?.ToLower().Contains(searchLower) ?? false) ||
                    (p.Content?.ToLower().Contains(searchLower) ?? false) ||
                    (p.Tags?.ToLower().Contains(searchLower) ?? false) ||
                    (p.AuthorName?.ToLower().Contains(searchLower) ?? false)
                ).ToList();

                result = new PagedResult<Domain.DTOs.Community.PostDto>(
                    filteredItems,
                    filteredItems.Count,
                    page,
                    pageSize);
            }

            ViewBag.CurrentType = type;
            ViewBag.GroupId = groupId;
            ViewBag.PostTypes = Enum.GetValues<PostType>();
            ViewBag.SearchTerm = search;

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading posts");
            TempData["Error"] = _localizer["FailedToLoadPosts"].Value;
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
                TempData["Error"] = _localizer["PostNotFound"].Value;
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
            TempData["Error"] = _localizer["FailedToLoadPost"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Post/Create
    [Authorize]
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.PostTypes = Enum.GetValues<PostType>();
        ViewBag.PostCategories = Enum.GetValues<PostCategory>();
        
        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            var userGroups = await _groupService.GetUserGroupsAsync(userId.Value, new QueryParameters { PageSize = 100 });
            ViewBag.UserGroups = userGroups.Items;
        }
        
        return View(new CreatePostViewModel());
    }

    // POST: Post/Create
    [Authorize]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(52428800)] // 50 MB limit
    public async Task<IActionResult> Create(CreatePostViewModel model)
    {
        try
        {
            // Validate file sizes
            if (model.ImageFile != null)
            {
                const long maxImageSize = 10 * 1024 * 1024; // 10 MB
                if (model.ImageFile.Length > maxImageSize)
                {
                    ModelState.AddModelError(nameof(model.ImageFile), "Image file size cannot exceed 10 MB");
                }
                
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var imageExtension = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
                if (!allowedImageExtensions.Contains(imageExtension))
                {
                    ModelState.AddModelError(nameof(model.ImageFile), "Only JPG, PNG, GIF, and WebP images are allowed");
                }
            }
            
            if (model.VideoFile != null)
            {
                const long maxVideoSize = 50 * 1024 * 1024; // 50 MB
                if (model.VideoFile.Length > maxVideoSize)
                {
                    ModelState.AddModelError(nameof(model.VideoFile), "Video file size cannot exceed 50 MB");
                }
                
                var allowedVideoExtensions = new[] { ".mp4", ".webm", ".ogg", ".mov" };
                var videoExtension = Path.GetExtension(model.VideoFile.FileName).ToLowerInvariant();
                if (!allowedVideoExtensions.Contains(videoExtension))
                {
                    ModelState.AddModelError(nameof(model.VideoFile), "Only MP4, WebM, OGG, and MOV videos are allowed");
                }
            }
            
            if (!ModelState.IsValid)
            {
                ViewBag.PostTypes = Enum.GetValues<PostType>();
                ViewBag.PostCategories = Enum.GetValues<PostCategory>();
                
                var userId = GetCurrentUserId();
                if (userId.HasValue)
                {
                    var userGroups = await _groupService.GetUserGroupsAsync(userId.Value, new QueryParameters { PageSize = 100 });
                    ViewBag.UserGroups = userGroups.Items;
                }
                
                return View(model);
            }

            var currentUserId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();

            // Handle image upload only for Image type posts
            if (model.Type == PostType.Image && model.ImageFile != null && model.ImageFile.Length > 0)
            {
                model.ImageUrl = await _fileStorageService.SaveFileAsync(
                    model.ImageFile, 
                    "uploads/posts/images",
                    $"{currentUserId}_{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}");
            }
            
            // Handle video upload only for Video type posts
            if (model.Type == PostType.Video && model.VideoFile != null && model.VideoFile.Length > 0)
            {
                model.VideoUrl = await _fileStorageService.SaveFileAsync(
                    model.VideoFile, 
                    "uploads/posts/videos",
                    $"{currentUserId}_{Guid.NewGuid()}{Path.GetExtension(model.VideoFile.FileName)}");
            }

            var post = await _postService.CreatePostAsync(
                model.Title,
                model.Content,
                model.Type,
                currentUserId,
                model.Category,
                model.GroupId,
                model.Status);
            
            // Update media URLs only if files were uploaded for the correct post type
            if ((model.Type == PostType.Image && !string.IsNullOrEmpty(model.ImageUrl)) || 
                (model.Type == PostType.Video && !string.IsNullOrEmpty(model.VideoUrl)))
            {
                post.SetMedia(model.ImageUrl, model.VideoUrl);
                await _postService.SaveChangesAsync();
            }
            
            // Update link information if provided
            if (!string.IsNullOrEmpty(model.LinkUrl))
            {
                post.SetLink(model.LinkUrl, model.LinkTitle, model.LinkDescription);
                await _postService.SaveChangesAsync();
            }
            
            // Update tags if provided
            if (!string.IsNullOrEmpty(model.Tags))
            {
                post.SetTags(model.Tags);
                await _postService.SaveChangesAsync();
            }
            
            // Update category
            post.SetCategory(model.Category);
            await _postService.SaveChangesAsync();

            // Send real-time notifications
            try
            {
                // Notify user about successful creation
                await _postHubService.NotifyPostCreatedAsync(currentUserId, new
                {
                    post.Id,
                    post.Title,
                    post.Content,
                    post.Type,
                    post.Status,
                    post.ImageUrl,
                    post.VideoUrl,
                    post.CreatedAt
                });

                // Notify friends if post is published
                if (post.Status == PostStatus.Published)
                {
                    var friends = await _friendshipService.GetFriendsAsync(currentUserId);
                    var friendIds = friends.Select(f => f.FriendId).ToList();
                    
                    if (friendIds.Any())
                    {
                        await _postHubService.NotifyFriendsNewPostAsync(friendIds, new
                        {
                            post.Id,
                            post.Title,
                            post.Content,
                            post.Type,
                            post.ImageUrl,
                            post.VideoUrl,
                            AuthorId = currentUserId,
                            post.CreatedAt
                        });
                    }
                }
            }
            catch (Exception hubEx)
            {
                _logger.LogError(hubEx, "Error sending post notifications");
                // Don't fail the request if notifications fail
            }

            TempData["SuccessToast"] = _localizer["PostCreated"].Value;
            return RedirectToAction(nameof(Details), new { slug = post.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            ModelState.AddModelError("", _localizer["FailedToCreatePost"].Value);
            ViewBag.PostTypes = Enum.GetValues<PostType>();
            ViewBag.PostCategories = Enum.GetValues<PostCategory>();
            
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                var userGroups = await _groupService.GetUserGroupsAsync(userId.Value, new QueryParameters { PageSize = 100 });
                ViewBag.UserGroups = userGroups.Items;
            }
            
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
                TempData["Error"] = _localizer["PostNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            if (!postDto.IsAuthor)
            {
                TempData["Error"] = _localizer["OnlyAuthorCanEditPost"].Value;
                return RedirectToAction(nameof(Details), new { slug = postDto.Slug });
            }

            var model = new EditPostViewModel
            {
                Id = postDto.Id,
                Title = postDto.Title,
                Content = postDto.Content,
                Type = postDto.Type,
                Status = postDto.Status,
                ImageUrl = postDto.ImageUrl,
                VideoUrl = postDto.VideoUrl,
                LinkUrl = postDto.LinkUrl,
                LinkTitle = postDto.LinkTitle,
                LinkDescription = postDto.LinkDescription,
                Tags = postDto.Tags,
                Category = postDto.Category
            };

            ViewBag.PostTypes = Enum.GetValues<PostType>();
            ViewBag.PostCategories = Enum.GetValues<PostCategory>();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading post for edit {PostId}", id);
            TempData["Error"] = _localizer["FailedToLoadPost"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Post/Edit/{id}
    [Authorize]
    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(52428800)] // 50 MB limit
    public async Task<IActionResult> Edit(Guid id, EditPostViewModel model)
    {
        try
        {
            if (id != model.Id)
                return BadRequest();
            
            var currentUserId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();

            // Validate file sizes
            if (model.ImageFile != null)
            {
                const long maxImageSize = 10 * 1024 * 1024; // 10 MB
                if (model.ImageFile.Length > maxImageSize)
                {
                    ModelState.AddModelError(nameof(model.ImageFile), "Image file size cannot exceed 10 MB");
                }
                
                var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var imageExtension = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
                if (!allowedImageExtensions.Contains(imageExtension))
                {
                    ModelState.AddModelError(nameof(model.ImageFile), "Only JPG, PNG, GIF, and WebP images are allowed");
                }
            }
            
            if (model.VideoFile != null)
            {
                const long maxVideoSize = 50 * 1024 * 1024; // 50 MB
                if (model.VideoFile.Length > maxVideoSize)
                {
                    ModelState.AddModelError(nameof(model.VideoFile), "Video file size cannot exceed 50 MB");
                }
                
                var allowedVideoExtensions = new[] { ".mp4", ".webm", ".ogg", ".mov" };
                var videoExtension = Path.GetExtension(model.VideoFile.FileName).ToLowerInvariant();
                if (!allowedVideoExtensions.Contains(videoExtension))
                {
                    ModelState.AddModelError(nameof(model.VideoFile), "Only MP4, WebM, OGG, and MOV videos are allowed");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PostTypes = Enum.GetValues<PostType>();
                ViewBag.PostCategories = Enum.GetValues<PostCategory>();
                return View(model);
            }

            // Handle image upload only for Image type posts
            if (model.Type == PostType.Image && model.ImageFile != null && model.ImageFile.Length > 0)
            {
                model.ImageUrl = await _fileStorageService.SaveFileAsync(
                    model.ImageFile, 
                    "uploads/posts/images",
                    $"{currentUserId}_{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}");
            }
            
            // Handle video upload only for Video type posts
            if (model.Type == PostType.Video && model.VideoFile != null && model.VideoFile.Length > 0)
            {
                model.VideoUrl = await _fileStorageService.SaveFileAsync(
                    model.VideoFile, 
                    "uploads/posts/videos",
                    $"{currentUserId}_{Guid.NewGuid()}{Path.GetExtension(model.VideoFile.FileName)}");
            }

            var post = await _postService.UpdatePostAsync(
                id,
                model.Title,
                model.Content,
                model.Type,
                model.Category,
                model.Status);
            
            // Update media URLs only if files were uploaded for the correct post type
            if ((model.Type == PostType.Image && !string.IsNullOrEmpty(model.ImageUrl)) || 
                (model.Type == PostType.Video && !string.IsNullOrEmpty(model.VideoUrl)))
            {
                post.SetMedia(model.ImageUrl, model.VideoUrl);
                await _postService.SaveChangesAsync();
            }
            
            // Update link information if provided
            if (!string.IsNullOrEmpty(model.LinkUrl))
            {
                post.SetLink(model.LinkUrl, model.LinkTitle, model.LinkDescription);
                await _postService.SaveChangesAsync();
            }
            
            // Update tags if provided
            if (!string.IsNullOrEmpty(model.Tags))
            {
                post.SetTags(model.Tags);
                await _postService.SaveChangesAsync();
            }

            // Send real-time notification about update
            try
            {
                await _postHubService.NotifyPostUpdatedAsync(currentUserId, new
                {
                    post.Id,
                    post.Title,
                    post.Content,
                    post.Type,
                    post.Status,
                    post.ImageUrl,
                    post.VideoUrl,
                    post.ModifiedAt
                });
            }
            catch (Exception hubEx)
            {
                _logger.LogError(hubEx, "Error sending post update notification");
                // Don't fail the request if notifications fail
            }

            TempData["SuccessToast"] = _localizer["PostUpdated"].Value;
            return RedirectToAction(nameof(Details), new { slug = post.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post {PostId}", id);
            ModelState.AddModelError("", _localizer["FailedToUpdatePost"].Value);
            ViewBag.PostTypes = Enum.GetValues<PostType>();
            ViewBag.PostCategories = Enum.GetValues<PostCategory>();
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

    // POST: Post/TogglePin/{id}
    [Authorize]
    [HttpPost("TogglePin/{id:guid}")]
    public async Task<IActionResult> TogglePin(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            var postDto = await _postService.GetPostByIdAsync(id, currentUserId);
            
            if (postDto == null)
                return Json(new { success = false, message = _localizer["PostNotFound"].Value });
            
            if (!postDto.IsAuthor)
                return Json(new { success = false, message = _localizer["OnlyAuthorCanPinPost"].Value });
            
            if (postDto.IsPinned)
            {
                await _postService.UnpinPostAsync(id);
                return Json(new { success = true, isPinned = false, message = _localizer["PostUnpinned"].Value });
            }
            else
            {
                await _postService.PinPostAsync(id);
                return Json(new { success = true, isPinned = true, message = _localizer["PostPinned"].Value });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling pin for post {PostId}", id);
            return Json(new { success = false, message = _localizer["FailedToTogglePin"].Value });
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
            var result = await _postService.ToggleLikeAsync(id, userId);

            return Json(new 
            { 
                success = true, 
                isLiked = result.IsLiked,
                totalLikes = result.TotalLikes,
                message = _localizer["LikeToggled"].Value 
            });
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

    // GET: Post/GetLikers/{id}
    [HttpGet("GetLikers/{id:guid}")]
    public async Task<IActionResult> GetLikers(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var likers = await _postService.GetPostLikersAsync(id, new QueryParameters { PageSize = 50 });
            
            var likersData = likers.Select(l => new
            {
                userId = l.UserId,
                userName = l.UserName,
                userAvatar = l.UserAvatar ?? "/images/default-avatar.svg"
            }).ToList();

            return Json(new { success = true, likers = likersData, total = likers.Count() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting likers for post {PostId}", id);
            return Json(new { success = false, message = _localizer["FailedToGetLikers"].Value });
        }
    }

    // POST: Post/AddReaction/{id}
    [Authorize]
    [HttpPost("AddReaction/{id:guid}")]
    public async Task<IActionResult> AddReaction(Guid id, string reactionType)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            
            // For now, treat reactions as likes
            var result = await _postService.ToggleLikeAsync(id, userId);

            return Json(new 
            { 
                success = true, 
                reactionType = reactionType,
                totalLikes = result.TotalLikes,
                message = _localizer["ReactionAdded"].Value 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding reaction to post {PostId}", id);
            return Json(new { success = false, message = _localizer["FailedToAddReaction"].Value });
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
            TempData["Error"] = _localizer["FailedToLoadMyPosts"].Value;
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
            TempData["Error"] = _localizer["FailedToLoadFeaturedPosts"].Value;
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
