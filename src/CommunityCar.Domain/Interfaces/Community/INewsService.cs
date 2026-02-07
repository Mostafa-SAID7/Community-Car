using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.news;
using CommunityCar.Domain.Enums.Community.news;

namespace CommunityCar.Domain.Interfaces.Community;

public interface INewsService
{
    // CRUD Operations
    Task<NewsArticle> CreateNewsArticleAsync(
        string title,
        string content,
        string summary,
        NewsCategory category,
        Guid authorId);
    
    Task<NewsArticle> UpdateNewsArticleAsync(
        Guid articleId,
        string title,
        string content,
        string summary,
        NewsCategory category);
    
    Task DeleteNewsArticleAsync(Guid articleId);
    
    // Query Operations
    Task<NewsArticleDto?> GetNewsArticleByIdAsync(Guid articleId, Guid? currentUserId = null);
    Task<NewsArticleDto?> GetNewsArticleBySlugAsync(string slug, Guid? currentUserId = null);
    
    Task<PagedResult<NewsArticleDto>> GetNewsArticlesAsync(
        QueryParameters parameters,
        NewsStatus? status = null,
        NewsCategory? category = null,
        bool? isFeatured = null,
        Guid? currentUserId = null);
    
    Task<PagedResult<NewsArticleDto>> GetUserNewsArticlesAsync(
        Guid userId,
        QueryParameters parameters,
        Guid? currentUserId = null);
    
    Task<List<NewsArticleDto>> GetFeaturedNewsAsync(int count = 5);
    Task<List<NewsArticleDto>> GetLatestNewsAsync(int count = 10);
    
    // Status Management
    Task PublishNewsArticleAsync(Guid articleId);
    Task ArchiveNewsArticleAsync(Guid articleId);
    Task FeatureNewsArticleAsync(Guid articleId);
    Task UnfeatureNewsArticleAsync(Guid articleId);
    
    // Engagement
    Task IncrementViewsAsync(Guid articleId);
    Task ToggleLikeAsync(Guid articleId, Guid userId);
    Task IncrementSharesAsync(Guid articleId);
    
    // Comments
    Task<NewsComment> AddCommentAsync(Guid articleId, Guid userId, string content);
    Task<NewsComment> UpdateCommentAsync(Guid commentId, Guid userId, string content);
    Task DeleteCommentAsync(Guid commentId, Guid userId);
    Task<PagedResult<NewsCommentDto>> GetNewsCommentsAsync(
        Guid articleId,
        QueryParameters parameters,
        Guid? currentUserId = null);
}
