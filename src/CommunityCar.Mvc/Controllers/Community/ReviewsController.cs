using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.reviews;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.ViewModels.Reviews;
using CommunityCar.Mvc.Attributes;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Community;

[Route("{culture:alpha}/Reviews")]
public class ReviewsController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly IFriendshipService _friendshipService;
    private readonly ILogger<ReviewsController> _logger;
    private readonly IStringLocalizer<ReviewsController> _localizer;

    public ReviewsController(
        IReviewService reviewService,
        IFriendshipService friendshipService,
        ILogger<ReviewsController> logger,
        IStringLocalizer<ReviewsController> localizer)
    {
        _reviewService = reviewService;
        _friendshipService = friendshipService;
        _logger = logger;
        _localizer = localizer;
    }

    // GET: Reviews
    [HttpGet("")]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 12,
        ReviewType? type = null,
        int? minRating = null,
        int? maxRating = null)
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var currentUserId = GetCurrentUserId();

            var result = await _reviewService.GetReviewsAsync(
                parameters,
                type,
                ReviewStatus.Approved,
                minRating,
                maxRating,
                currentUserId);

            ViewBag.CurrentType = type;
            ViewBag.MinRating = minRating;
            ViewBag.MaxRating = maxRating;
            ViewBag.Types = Enum.GetValues<ReviewType>();

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reviews");
            TempData["Error"] = _localizer["FailedToLoadReviews"].Value;
            return View(new PagedResult<Domain.DTOs.Community.ReviewDto>(
                new List<Domain.DTOs.Community.ReviewDto>(), 0, page, pageSize));
        }
    }

    // GET: Reviews/Details/{slug}
    [HttpGet("Details/{slug}")]
    [HttpGet("Details")]
    public async Task<IActionResult> Details(string slug)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var reviewDto = await _reviewService.GetReviewBySlugAsync(slug, currentUserId);

            if (reviewDto == null)
            {
                TempData["Error"] = _localizer["ReviewNotFound"].Value;
                return RedirectToAction(nameof(Index));
            }

            var commentsParams = new QueryParameters { PageNumber = 1, PageSize = 10 };
            var comments = await _reviewService.GetReviewCommentsAsync(
                reviewDto.Id,
                commentsParams,
                currentUserId);

            var ratingDistribution = await _reviewService.GetRatingDistributionAsync(
                reviewDto.EntityId,
                reviewDto.EntityType);

            var averageRating = await _reviewService.GetAverageRatingAsync(
                reviewDto.EntityId,
                reviewDto.EntityType);

            var viewModel = new ReviewDetailsViewModel
            {
                Review = reviewDto,
                Comments = comments,
                RatingDistribution = ratingDistribution,
                AverageRating = averageRating,
                TotalReviews = ratingDistribution.Values.Sum()
            };

            if (currentUserId.HasValue)
            {
                var status = await _friendshipService.GetFriendshipStatusAsync(currentUserId.Value, reviewDto.ReviewerId);
                ViewBag.FriendshipStatus = status;
                
                if (status == CommunityCar.Domain.Enums.Community.friends.FriendshipStatus.Pending)
                {
                    var pendingRequests = await _friendshipService.GetPendingRequestsAsync(currentUserId.Value);
                    ViewBag.IsIncomingRequest = pendingRequests.Any(r => r.UserId == reviewDto.ReviewerId);
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
            _logger.LogError(ex, "Error loading review {Slug}", slug);
            TempData["Error"] = _localizer["FailedToLoadReview"].Value;
            return RedirectToAction(nameof(Index));
        }
    }


    // GET: Reviews/Create
    [Authorize]
    [HttpGet("Create")]
    public IActionResult Create(Guid? entityId, string? entityType)
    {
        var model = new CreateReviewViewModel
        {
            EntityId = entityId,
            EntityType = entityType
        };

        ViewBag.Types = Enum.GetValues<ReviewType>();
        return View(model);
    }

    // POST: Reviews/Create
    [Authorize]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    // Temporarily increased for development/testing - reduce in production
    [RateLimit("CreateReview", maxRequests: 100, timeWindowSeconds: 300)] // Max 100 reviews per 5 minutes (for testing)
    public async Task<IActionResult> Create(CreateReviewViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                ViewBag.Types = Enum.GetValues<ReviewType>();
                return View(model);
            }

            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();

            // For standalone reviews (no entity), use a generated GUID
            var targetEntityId = model.EntityId ?? Guid.NewGuid();
            var targetEntityType = model.EntityType ?? "General";

            // Check for duplicate review only if reviewing a specific entity
            if (model.EntityId.HasValue && !string.IsNullOrEmpty(model.EntityType))
            {
                var existingReview = await _reviewService.GetUserReviewForEntityAsync(userId, targetEntityId, targetEntityType);
                if (existingReview != null)
                {
                    var errorMessage = _localizer["YouHaveAlreadyReviewedThisItem"].Value;
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = errorMessage });
                    }

                    ModelState.AddModelError("", errorMessage);
                    ViewBag.Types = Enum.GetValues<ReviewType>();
                    return View(model);
                }
            }

            var review = await _reviewService.CreateReviewAsync(
                targetEntityId,
                targetEntityType,
                model.Type,
                userId,
                model.Rating,
                model.Title,
                model.Content,
                model.Pros,
                model.Cons,
                model.IsVerifiedPurchase,
                model.IsRecommended,
                model.GroupId);

            _logger.LogInformation("User {UserId} created review {ReviewId} for {EntityType} {EntityId}",
                userId, review.Id, targetEntityType, targetEntityId);

            var successMessage = _localizer["ReviewSubmittedForApproval"].Value;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    message = successMessage,
                    reviewId = review.Id,
                    slug = review.Slug
                });
            }

            TempData["Success"] = successMessage;
            return RedirectToAction(nameof(Details), new { slug = review.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review for user {UserId}", GetCurrentUserId());
            
            var errorMessage = _localizer["FailedToCreateReview"].Value;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMessage });
            }

            ModelState.AddModelError("", errorMessage);
            ViewBag.Types = Enum.GetValues<ReviewType>();
            return View(model);
        }
    }

    // GET: Reviews/Edit/{id}
    [Authorize]
    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            var reviewDto = await _reviewService.GetReviewByIdAsync(id, userId);

            if (reviewDto == null)
            {
                TempData["Error"] = "Review not found";
                return RedirectToAction(nameof(Index));
            }

            if (!reviewDto.IsReviewer)
            {
                TempData["Error"] = _localizer["OnlyReviewerCanEdit"].Value;
                return RedirectToAction(nameof(Details), new { slug = reviewDto.Slug });
            }

            var model = new EditReviewViewModel
            {
                Id = reviewDto.Id,
                Rating = reviewDto.Rating,
                Title = reviewDto.Title,
                Content = reviewDto.Content,
                Pros = reviewDto.Pros,
                Cons = reviewDto.Cons,
                IsRecommended = reviewDto.IsRecommended
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading review for edit {ReviewId}", id);
            TempData["Error"] = _localizer["FailedToLoadReview"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Reviews/Edit/{id}
    [Authorize]
    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    [RateLimit("EditReview", maxRequests: 5, timeWindowSeconds: 300)] // Max 5 edits per 5 minutes
    public async Task<IActionResult> Edit(Guid id, EditReviewViewModel model)
    {
        try
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }
                return View(model);
            }

            var review = await _reviewService.UpdateReviewAsync(
                id,
                model.Rating,
                model.Title,
                model.Content,
                model.Pros,
                model.Cons,
                model.IsRecommended);

            _logger.LogInformation("User {UserId} updated review {ReviewId}",
                GetCurrentUserId(), id);

            var successMessage = _localizer["ReviewUpdated"].Value;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    message = successMessage,
                    slug = review.Slug
                });
            }

            TempData["Success"] = successMessage;
            return RedirectToAction(nameof(Details), new { slug = review.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review {ReviewId}", id);
            
            var errorMessage = _localizer["FailedToUpdateReview"].Value;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMessage });
            }

            ModelState.AddModelError("", errorMessage);
            return View(model);
        }
    }

    // POST: Reviews/Delete/{id}
    [Authorize]
    [HttpPost("Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _reviewService.DeleteReviewAsync(id);
            return Json(new { success = true, message = _localizer["ReviewDeleted"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", id);
            return Json(new { success = false, message = _localizer["FailedToDeleteReview"].Value });
        }
    }

    // POST: Reviews/MarkHelpful/{id}
    [Authorize]
    [HttpPost("MarkHelpful/{id:guid}")]
    [RateLimit("MarkHelpful", maxRequests: 10, timeWindowSeconds: 60)] // Max 10 reactions per minute
    public async Task<IActionResult> MarkHelpful(Guid id, bool isHelpful)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _reviewService.MarkReviewHelpfulAsync(id, userId, isHelpful);

            _logger.LogInformation("User {UserId} marked review {ReviewId} as {Helpful}",
                userId, id, isHelpful ? "helpful" : "not helpful");

            return Json(new { success = true, message = _localizer["FeedbackReceived"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking review {ReviewId} as helpful", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST: Reviews/RemoveReaction/{id}
    [Authorize]
    [HttpPost("RemoveReaction/{id:guid}")]
    public async Task<IActionResult> RemoveReaction(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _reviewService.RemoveReviewReactionAsync(id, userId);

            return Json(new { success = true, message = _localizer["ReactionRemoved"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reaction from review {ReviewId}", id);
            return Json(new { success = false, message = _localizer["FailedToRemoveReaction"].Value });
        }
    }

    // POST: Reviews/Flag/{id}
    [Authorize]
    [HttpPost("Flag/{id:guid}")]
    [RateLimit("FlagReview", maxRequests: 5, timeWindowSeconds: 300)] // Max 5 flags per 5 minutes
    public async Task<IActionResult> Flag(Guid id, string? reason = null)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _reviewService.FlagReviewAsync(id, userId, reason);

            _logger.LogWarning("User {UserId} flagged review {ReviewId} with reason: {Reason}",
                userId, id, reason ?? "No reason provided");

            return Json(new { success = true, message = _localizer["ReviewFlagged"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flagging review {ReviewId}", id);
            return Json(new { success = false, message = _localizer["FailedToFlagReview"].Value });
        }
    }

    // POST: Reviews/AddComment
    [Authorize]
    [HttpPost("AddComment")]
    [RateLimit("AddComment", maxRequests: 10, timeWindowSeconds: 60)] // Max 10 comments per minute
    public async Task<IActionResult> AddComment(Guid reviewId, string content)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _reviewService.AddCommentAsync(reviewId, userId, content);

            _logger.LogInformation("User {UserId} added comment to review {ReviewId}",
                userId, reviewId);

            return Json(new { success = true, message = _localizer["CommentAdded"].Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to review {ReviewId}", reviewId);
            return Json(new { success = false, message = _localizer["FailedToAddComment"].Value });
        }
    }

    // GET: Reviews/MyReviews
    [Authorize]
    [HttpGet("MyReviews")]
    public async Task<IActionResult> MyReviews(int page = 1, int pageSize = 12)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var result = await _reviewService.GetUserReviewsAsync(userId, parameters, userId);

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user reviews");
            TempData["Error"] = _localizer["FailedToLoadMyReviews"].Value;
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Reviews/Entity/{entityId}
    [HttpGet("Entity/{entityId:guid}")]
    public async Task<IActionResult> EntityReviews(Guid entityId, string entityType, int page = 1, int pageSize = 10)
    {
        try
        {
            var parameters = new QueryParameters { PageNumber = page, PageSize = pageSize };
            var currentUserId = GetCurrentUserId();
            
            var result = await _reviewService.GetReviewsByEntityAsync(
                entityId,
                entityType,
                parameters,
                currentUserId);

            var ratingDistribution = await _reviewService.GetRatingDistributionAsync(entityId, entityType);
            var averageRating = await _reviewService.GetAverageRatingAsync(entityId, entityType);

            ViewBag.EntityId = entityId;
            ViewBag.EntityType = entityType;
            ViewBag.RatingDistribution = ratingDistribution;
            ViewBag.AverageRating = averageRating;
            ViewBag.TotalReviews = ratingDistribution.Values.Sum();

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading entity reviews");
            TempData["Error"] = _localizer["FailedToLoadReviews"].Value;
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
