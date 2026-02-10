using AutoMapper;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.reviews;
using CommunityCar.Domain.Enums.Community.reviews;
using CommunityCar.Domain.Exceptions;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

/// <summary>
/// Review service using Unit of Work pattern for better transaction management
/// </summary>
public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<ReviewService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Review> CreateReviewAsync(
        Guid entityId,
        string entityType,
        ReviewType type,
        Guid reviewerId,
        decimal rating,
        string title,
        string content,
        string? pros = null,
        string? cons = null,
        bool isVerifiedPurchase = false,
        bool isRecommended = true,
        Guid? groupId = null)
    {
        var review = new Review(
            entityId,
            entityType,
            type,
            reviewerId,
            rating,
            title,
            content,
            isVerifiedPurchase,
            isRecommended,
            groupId);

        // Set optional properties
        if (!string.IsNullOrEmpty(pros) || !string.IsNullOrEmpty(cons))
            review.SetProsAndCons(pros, cons);

        await _uow.Repository<Review>().AddAsync(review);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Review created: {ReviewId} by user {UserId}", review.Id, reviewerId);
        return review;
    }

    public async Task<Review> UpdateReviewAsync(
        Guid reviewId,
        decimal rating,
        string title,
        string content,
        string? pros,
        string? cons,
        bool isRecommended)
    {
        var review = await _uow.Repository<Review>()
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            throw new NotFoundException("Review not found");

        review.Update(rating, title, content, isRecommended);
        review.SetProsAndCons(pros, cons);

        _uow.Repository<Review>().Update(review);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Review updated: {ReviewId}", reviewId);
        return review;
    }

    public async Task DeleteReviewAsync(Guid reviewId)
    {
        var review = await _uow.Repository<Review>()
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            throw new NotFoundException("Review not found");

        _uow.Repository<Review>().Delete(review);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Review deleted: {ReviewId}", reviewId);
    }

    public async Task<ReviewDto?> GetReviewByIdAsync(Guid reviewId, Guid? currentUserId = null)
    {
        var query = _uow.Repository<Review>().GetQueryable();
        var review = await query
            .Include(r => r.Reviewer)
            .Include(r => r.Reactions)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            return null;

        return MapToDto(review, currentUserId);
    }

    public async Task<ReviewDto?> GetReviewBySlugAsync(string slug, Guid? currentUserId = null)
    {
        var query = _uow.Repository<Review>().GetQueryable();
        var review = await query
            .Include(r => r.Reviewer)
            .Include(r => r.Reactions)
            .FirstOrDefaultAsync(r => r.Slug == slug);

        if (review == null)
            return null;

        return MapToDto(review, currentUserId);
    }

    public async Task<ReviewDto?> GetUserReviewForEntityAsync(Guid userId, Guid entityId, string entityType)
    {
        var query = _uow.Repository<Review>().GetQueryable();
        var review = await query
            .Include(r => r.Reviewer)
            .Include(r => r.Reactions)
            .FirstOrDefaultAsync(r => r.ReviewerId == userId && r.EntityId == entityId && r.EntityType == entityType);

        if (review == null)
            return null;

        return MapToDto(review, userId);
    }

    public async Task<bool> HasUserReviewedEntityAsync(Guid userId, Guid entityId, string entityType)
    {
        return await _uow.Repository<Review>()
            .GetQueryable()
            .AnyAsync(r => r.ReviewerId == userId && r.EntityId == entityId && r.EntityType == entityType);
    }

    public async Task<bool> CanUserReviewAsync(Guid userId)
    {
        // Check if user has created more than 3 reviews in the last 5 minutes (rate limiting)
        var fiveMinutesAgo = DateTimeOffset.UtcNow.AddMinutes(-5);
        var recentReviewCount = await _uow.Repository<Review>()
            .CountAsync(r => r.ReviewerId == userId && r.CreatedAt >= fiveMinutesAgo);

        return recentReviewCount < 3;
    }

    public async Task<PagedResult<ReviewDto>> GetReviewsAsync(
        QueryParameters parameters,
        ReviewType? type = null,
        ReviewStatus? status = null,
        int? minRating = null,
        int? maxRating = null,
        Guid? currentUserId = null)
    {
        IQueryable<Review> query = _uow.Repository<Review>().GetQueryable()
            .Include(r => r.Reviewer)
            .Include(r => r.Reactions);

        if (type.HasValue)
            query = query.Where(r => r.Type == type.Value);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);
        else
            query = query.Where(r => r.Status == ReviewStatus.Approved);

        if (minRating.HasValue)
            query = query.Where(r => r.Rating.Value >= minRating.Value);

        if (maxRating.HasValue)
            query = query.Where(r => r.Rating.Value <= maxRating.Value);

        query = query.OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(r => MapToDto(r, currentUserId)).ToList();

        return new PagedResult<ReviewDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<ReviewDto>> GetReviewsByEntityAsync(
        Guid entityId,
        string entityType,
        QueryParameters parameters,
        Guid? currentUserId = null)
    {
        var query = _uow.Repository<Review>().GetQueryable()
            .Include(r => r.Reviewer)
            .Include(r => r.Reactions)
            .Where(r => r.EntityId == entityId && r.EntityType == entityType && r.Status == ReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(r => MapToDto(r, currentUserId)).ToList();

        return new PagedResult<ReviewDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<ReviewDto>> GetUserReviewsAsync(
        Guid userId,
        QueryParameters parameters,
        Guid? currentUserId = null)
    {
        var query = _uow.Repository<Review>().GetQueryable()
            .Include(r => r.Reviewer)
            .Include(r => r.Reactions)
            .Where(r => r.ReviewerId == userId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(r => MapToDto(r, currentUserId)).ToList();

        return new PagedResult<ReviewDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<ReviewStatisticsDto> GetEntityReviewStatisticsAsync(Guid entityId, string entityType)
    {
        var reviews = await _uow.Repository<Review>()
            .WhereAsync(r => r.EntityId == entityId && r.EntityType == entityType && r.Status == ReviewStatus.Approved);

        var reviewList = reviews.ToList();
        var totalReviews = reviewList.Count;
        var averageRating = totalReviews > 0 ? reviewList.Average(r => r.Rating.Value) : 0;
        var recommendedCount = reviewList.Count(r => r.IsRecommended);

        return new ReviewStatisticsDto
        {
            TotalReviews = totalReviews,
            AverageRating = (double)Math.Round(averageRating, 2),
            FiveStarCount = reviewList.Count(r => r.Rating.Value == 5),
            FourStarCount = reviewList.Count(r => r.Rating.Value == 4),
            ThreeStarCount = reviewList.Count(r => r.Rating.Value == 3),
            TwoStarCount = reviewList.Count(r => r.Rating.Value == 2),
            OneStarCount = reviewList.Count(r => r.Rating.Value == 1),
            RecommendedCount = recommendedCount,
            RecommendedPercentage = totalReviews > 0 ? Math.Round((double)recommendedCount / totalReviews * 100, 2) : 0
        };
    }

    public async Task ApproveReviewAsync(Guid reviewId, Guid moderatorId)
    {
        var review = await _uow.Repository<Review>()
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            throw new NotFoundException("Review not found");

        review.Approve(moderatorId);
        _uow.Repository<Review>().Update(review);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Review approved: {ReviewId} by moderator {ModeratorId}", reviewId, moderatorId);
    }

    public async Task RejectReviewAsync(Guid reviewId, Guid moderatorId, string reason)
    {
        var review = await _uow.Repository<Review>()
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            throw new NotFoundException("Review not found");

        review.Reject(moderatorId, reason);
        _uow.Repository<Review>().Update(review);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Review rejected: {ReviewId} by moderator {ModeratorId}", reviewId, moderatorId);
    }

    public async Task FlagReviewAsync(Guid reviewId, Guid userId, string? reason = null)
    {
        var review = await _uow.Repository<Review>()
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            throw new NotFoundException("Review not found");

        review.Flag(reason ?? "Flagged by user");
        _uow.Repository<Review>().Update(review);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Review flagged: {ReviewId} by user {UserId}", reviewId, userId);
    }

    public async Task MarkReviewHelpfulAsync(Guid reviewId, Guid userId, bool isHelpful)
    {
        await MarkAsHelpfulAsync(reviewId, userId, isHelpful);
    }

    public async Task MarkAsHelpfulAsync(Guid reviewId, Guid userId, bool isHelpful)
    {
        var query = _uow.Repository<Review>().GetQueryable();
        var review = await query
            .Include(r => r.Reactions)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
            throw new NotFoundException("Review not found");

        var existingReaction = review.Reactions.FirstOrDefault(r => r.UserId == userId);

        if (existingReaction != null)
        {
            // Update existing reaction
            var oldValue = existingReaction.IsHelpful;
            existingReaction.UpdateReaction(isHelpful);

            // Update counts
            if (oldValue && !isHelpful)
            {
                review.MarkNotHelpful();
            }
            else if (!oldValue && isHelpful)
            {
                review.MarkHelpful();
            }
        }
        else
        {
            // Create new reaction
            var reaction = new ReviewReaction(reviewId, userId, isHelpful);
            await _uow.Repository<ReviewReaction>().AddAsync(reaction);

            if (isHelpful)
                review.MarkHelpful();
            else
                review.MarkNotHelpful();
        }

        await _uow.SaveChangesAsync();
        _logger.LogInformation("Review reaction updated: {ReviewId} by user {UserId}", reviewId, userId);
    }

    public async Task RemoveReviewReactionAsync(Guid reviewId, Guid userId)
    {
        await RemoveReactionAsync(reviewId, userId);
    }

    public async Task RemoveReactionAsync(Guid reviewId, Guid userId)
    {
        var reaction = await _uow.Repository<ReviewReaction>()
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.UserId == userId);

        if (reaction == null)
            throw new NotFoundException("Reaction not found");

        _uow.Repository<ReviewReaction>().Delete(reaction);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Review reaction removed: {ReviewId} by user {UserId}", reviewId, userId);
    }

    public async Task<ReviewComment> AddCommentAsync(Guid reviewId, Guid userId, string content)
    {
        var comment = new ReviewComment(reviewId, userId, content);
        await _uow.Repository<ReviewComment>().AddAsync(comment);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Comment added to review {ReviewId} by user {UserId}", reviewId, userId);
        return comment;
    }

    public async Task<ReviewComment> UpdateCommentAsync(Guid commentId, Guid userId, string content)
    {
        var comment = await _uow.Repository<ReviewComment>()
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new NotFoundException("Comment not found");

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only edit your own comments");

        comment.Update(content);
        _uow.Repository<ReviewComment>().Update(comment);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} updated", commentId);
        return comment;
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _uow.Repository<ReviewComment>()
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new NotFoundException("Comment not found");

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only delete your own comments");

        _uow.Repository<ReviewComment>().Delete(comment);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} deleted", commentId);
    }

    public async Task<PagedResult<ReviewCommentDto>> GetReviewCommentsAsync(
        Guid reviewId,
        QueryParameters parameters,
        Guid? currentUserId = null)
    {
        var query = _uow.Repository<ReviewComment>().GetQueryable()
            .Include(c => c.User)
            .Where(c => c.ReviewId == reviewId)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(c => new ReviewCommentDto
        {
            Id = c.Id,
            ReviewId = c.ReviewId,
            UserId = c.UserId,
            UserName = c.User.UserName ?? "Unknown",
            UserAvatar = c.User.ProfilePictureUrl,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.ModifiedAt,
            IsAuthor = currentUserId.HasValue && c.UserId == currentUserId.Value
        }).ToList();

        return new PagedResult<ReviewCommentDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid entityId, string entityType)
    {
        var reviews = await _uow.Repository<Review>()
            .WhereAsync(r => r.EntityId == entityId && r.EntityType == entityType && r.Status == ReviewStatus.Approved);

        var reviewList = reviews.ToList();

        return new Dictionary<int, int>
        {
            { 5, reviewList.Count(r => r.Rating.Value >= 4.5m && r.Rating.Value <= 5m) },
            { 4, reviewList.Count(r => r.Rating.Value >= 3.5m && r.Rating.Value < 4.5m) },
            { 3, reviewList.Count(r => r.Rating.Value >= 2.5m && r.Rating.Value < 3.5m) },
            { 2, reviewList.Count(r => r.Rating.Value >= 1.5m && r.Rating.Value < 2.5m) },
            { 1, reviewList.Count(r => r.Rating.Value >= 0m && r.Rating.Value < 1.5m) }
        };
    }

    public async Task<double> GetAverageRatingAsync(Guid entityId, string entityType)
    {
        var reviews = await _uow.Repository<Review>()
            .WhereAsync(r => r.EntityId == entityId && r.EntityType == entityType && r.Status == ReviewStatus.Approved);

        var reviewList = reviews.ToList();

        if (reviewList.Count == 0)
            return 0;

        return (double)Math.Round(reviewList.Average(r => r.Rating.Value), 2);
    }

    private ReviewDto MapToDto(Review review, Guid? currentUserId)
    {
        var currentUserReaction = currentUserId.HasValue
            ? review.Reactions.FirstOrDefault(r => r.UserId == currentUserId.Value)
            : null;

        return new ReviewDto
        {
            Id = review.Id,
            Slug = review.Slug,
            EntityId = review.EntityId,
            EntityType = review.EntityType,
            Type = review.Type,
            TypeName = review.Type.ToString(),
            Status = review.Status,
            StatusName = review.Status.ToString(),
            ReviewerId = review.ReviewerId,
            ReviewerName = review.Reviewer?.UserName ?? "Unknown",
            AuthorName = review.Reviewer?.UserName ?? "Unknown",
            ReviewerAvatar = review.Reviewer?.ProfilePictureUrl,
            Rating = review.Rating.Value,
            Title = review.Title,
            Comment = review.Comment,
            Content = review.Comment, // Map Comment to Content as well
            Pros = review.Pros,
            Cons = review.Cons,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            IsRecommended = review.IsRecommended,
            HelpfulCount = review.HelpfulCount,
            NotHelpfulCount = review.NotHelpfulCount,
            HelpfulPercentage = review.HelpfulPercentage,
            ImageUrls = review.ImageUrls,
            IsReviewer = currentUserId.HasValue && review.ReviewerId == currentUserId.Value,
            CurrentUserReaction = currentUserReaction?.IsHelpful,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.ModifiedAt
        };
    }
}

