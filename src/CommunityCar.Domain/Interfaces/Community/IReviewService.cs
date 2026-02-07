using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.reviews;
using CommunityCar.Domain.Enums.Community.reviews;

namespace CommunityCar.Domain.Interfaces.Community;

public interface IReviewService
{
    // CRUD Operations
    Task<Review> CreateReviewAsync(
        Guid entityId,
        string entityType,
        ReviewType type,
        Guid reviewerId,
        int rating,
        string title,
        string content,
        string? pros = null,
        string? cons = null,
        bool isVerifiedPurchase = false,
        bool isRecommended = true);
    
    Task<Review> UpdateReviewAsync(
        Guid reviewId,
        int rating,
        string title,
        string content,
        string? pros,
        string? cons,
        bool isRecommended);
    
    Task DeleteReviewAsync(Guid reviewId);
    
    // Query Operations
    Task<ReviewDto?> GetReviewByIdAsync(Guid reviewId, Guid? currentUserId = null);
    Task<ReviewDto?> GetReviewBySlugAsync(string slug, Guid? currentUserId = null);
    
    Task<PagedResult<ReviewDto>> GetReviewsAsync(
        QueryParameters parameters,
        ReviewType? type = null,
        ReviewStatus? status = null,
        int? minRating = null,
        int? maxRating = null,
        Guid? currentUserId = null);
    
    Task<PagedResult<ReviewDto>> GetReviewsByEntityAsync(
        Guid entityId,
        string entityType,
        QueryParameters parameters,
        Guid? currentUserId = null);
    
    Task<PagedResult<ReviewDto>> GetUserReviewsAsync(
        Guid userId,
        QueryParameters parameters,
        Guid? currentUserId = null);
    
    // Statistics
    Task<ReviewStatisticsDto> GetEntityReviewStatisticsAsync(Guid entityId, string entityType);
    
    // Moderation
    Task ApproveReviewAsync(Guid reviewId, Guid moderatorId);
    Task RejectReviewAsync(Guid reviewId, Guid moderatorId, string reason);
    Task FlagReviewAsync(Guid reviewId, Guid userId, string? reason = null);
    
    // Reactions
    Task MarkReviewHelpfulAsync(Guid reviewId, Guid userId, bool isHelpful);
    Task MarkAsHelpfulAsync(Guid reviewId, Guid userId, bool isHelpful); // Alias for consistency
    Task RemoveReviewReactionAsync(Guid reviewId, Guid userId);
    Task RemoveReactionAsync(Guid reviewId, Guid userId); // Alias for consistency
    
    // Additional Statistics
    Task<Dictionary<int, int>> GetRatingDistributionAsync(Guid entityId, string entityType);
    Task<double> GetAverageRatingAsync(Guid entityId, string entityType);
    
    // Comments
    Task<ReviewComment> AddCommentAsync(Guid reviewId, Guid userId, string content);
    Task<ReviewComment> UpdateCommentAsync(Guid commentId, Guid userId, string content);
    Task DeleteCommentAsync(Guid commentId, Guid userId);
    Task<PagedResult<ReviewCommentDto>> GetReviewCommentsAsync(
        Guid reviewId,
        QueryParameters parameters,
        Guid? currentUserId = null);
}
