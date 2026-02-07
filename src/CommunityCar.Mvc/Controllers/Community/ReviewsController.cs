using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Community.reviews;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.ViewModels.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommunityCar.Mvc.Controllers.Community;

[Route("Reviews")]
public class ReviewsController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(
        IReviewService reviewService,
        ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
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
            TempData["Error"] = "Failed to load reviews";
            return View(new PagedResult<Domain.DTOs.Community.ReviewDto>(
                new List<Domain.DTOs.Community.ReviewDto>(), 0, page, pageSize));
        }
    }

    // GET: Reviews/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var reviewDto = await _reviewService.GetReviewBySlugAsync(slug, currentUserId);

            if (reviewDto == null)
            {
                TempData["Error"] = "Review not found";
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

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading review {Slug}", slug);
            TempData["Error"] = "Failed to load review";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Reviews/Create
    [Authorize]
    [HttpGet("Create")]
    public IActionResult Create(Guid? entityId, string? entityType, ReviewType? type)
    {
        var model = new CreateReviewViewModel();
        
        if (entityId.HasValue)
            model.EntityId = entityId.Value;
        
        if (!string.IsNullOrEmpty(entityType))
            model.EntityType = entityType;
        
        if (type.HasValue)
            model.Type = type.Value;

        ViewBag.Types = Enum.GetValues<ReviewType>();
        return View(model);
    }

    // POST: Reviews/Create
    [Authorize]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReviewViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Types = Enum.GetValues<ReviewType>();
                return View(model);
            }

            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();

            var review = await _reviewService.CreateReviewAsync(
                model.EntityId,
                model.EntityType,
                model.Type,
                userId,
                model.Rating,
                model.Title,
                model.Content,
                model.Pros,
                model.Cons,
                model.IsVerifiedPurchase,
                model.IsRecommended);

            TempData["Success"] = "Review submitted successfully and is pending approval";
            return RedirectToAction(nameof(Details), new { slug = review.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            ModelState.AddModelError("", "Failed to create review");
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
                TempData["Error"] = "You can only edit your own reviews";
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
            TempData["Error"] = "Failed to load review";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Reviews/Edit/{id}
    [Authorize]
    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditReviewViewModel model)
    {
        try
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var review = await _reviewService.UpdateReviewAsync(
                id,
                model.Rating,
                model.Title,
                model.Content,
                model.Pros,
                model.Cons,
                model.IsRecommended);

            TempData["Success"] = "Review updated successfully";
            return RedirectToAction(nameof(Details), new { slug = review.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review {ReviewId}", id);
            ModelState.AddModelError("", "Failed to update review");
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
            return Json(new { success = true, message = "Review deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", id);
            return Json(new { success = false, message = "Failed to delete review" });
        }
    }

    // POST: Reviews/MarkHelpful/{id}
    [Authorize]
    [HttpPost("MarkHelpful/{id:guid}")]
    public async Task<IActionResult> MarkHelpful(Guid id, bool isHelpful)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _reviewService.MarkReviewHelpfulAsync(id, userId, isHelpful);

            return Json(new { success = true, message = "Thank you for your feedback" });
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

            return Json(new { success = true, message = "Reaction removed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reaction from review {ReviewId}", id);
            return Json(new { success = false, message = "Failed to remove reaction" });
        }
    }

    // POST: Reviews/Flag/{id}
    [Authorize]
    [HttpPost("Flag/{id:guid}")]
    public async Task<IActionResult> Flag(Guid id, string? reason = null)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _reviewService.FlagReviewAsync(id, userId, reason);

            return Json(new { success = true, message = "Review flagged for moderation" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error flagging review {ReviewId}", id);
            return Json(new { success = false, message = "Failed to flag review" });
        }
    }

    // POST: Reviews/AddComment
    [Authorize]
    [HttpPost("AddComment")]
    public async Task<IActionResult> AddComment(Guid reviewId, string content)
    {
        try
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException();
            await _reviewService.AddCommentAsync(reviewId, userId, content);

            return Json(new { success = true, message = "Comment added successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to review {ReviewId}", reviewId);
            return Json(new { success = false, message = "Failed to add comment" });
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
            TempData["Error"] = "Failed to load your reviews";
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
            TempData["Error"] = "Failed to load reviews";
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
