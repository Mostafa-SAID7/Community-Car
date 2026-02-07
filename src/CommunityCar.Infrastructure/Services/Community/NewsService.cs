using AutoMapper;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.news;
using CommunityCar.Domain.Enums.Community.news;
using CommunityCar.Domain.Exceptions;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

public class NewsService : INewsService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<NewsService> _logger;

    public NewsService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<NewsService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<NewsArticle> CreateNewsArticleAsync(
        string title,
        string content,
        string summary,
        NewsCategory category,
        Guid authorId)
    {
        var article = new NewsArticle(title, content, summary, category, authorId);
        
        _context.Set<NewsArticle>().Add(article);
        await _context.SaveChangesAsync();

        _logger.LogInformation("News article created: {ArticleId} by user {UserId}", article.Id, authorId);
        return article;
    }

    public async Task<NewsArticle> UpdateNewsArticleAsync(
        Guid articleId,
        string title,
        string content,
        string summary,
        NewsCategory category)
    {
        var article = await _context.Set<NewsArticle>()
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            throw new NotFoundException("News article not found");

        article.Update(title, content, summary, category);
        await _context.SaveChangesAsync();

        _logger.LogInformation("News article updated: {ArticleId}", articleId);
        return article;
    }

    public async Task DeleteNewsArticleAsync(Guid articleId)
    {
        var article = await _context.Set<NewsArticle>()
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            throw new NotFoundException("News article not found");

        _context.Set<NewsArticle>().Remove(article);
        await _context.SaveChangesAsync();

        _logger.LogInformation("News article deleted: {ArticleId}", articleId);
    }

    public async Task<NewsArticleDto?> GetNewsArticleByIdAsync(Guid articleId, Guid? currentUserId = null)
    {
        var article = await _context.Set<NewsArticle>()
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            return null;

        return await MapToDtoAsync(article, currentUserId);
    }

    public async Task<NewsArticleDto?> GetNewsArticleBySlugAsync(string slug, Guid? currentUserId = null)
    {
        var article = await _context.Set<NewsArticle>()
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Slug == slug);

        if (article == null)
            return null;

        return await MapToDtoAsync(article, currentUserId);
    }

    public async Task<PagedResult<NewsArticleDto>> GetNewsArticlesAsync(
        QueryParameters parameters,
        NewsStatus? status = null,
        NewsCategory? category = null,
        bool? isFeatured = null,
        Guid? currentUserId = null)
    {
        var query = _context.Set<NewsArticle>()
            .Include(a => a.Author)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);
        else
            query = query.Where(a => a.Status == NewsStatus.Published);

        if (category.HasValue)
            query = query.Where(a => a.Category == category.Value);

        if (isFeatured.HasValue)
            query = query.Where(a => a.IsFeatured == isFeatured.Value);

        query = query.OrderByDescending(a => a.PublishedAt ?? a.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = new List<NewsArticleDto>();
        foreach (var article in items)
        {
            dtos.Add(await MapToDtoAsync(article, currentUserId));
        }

        return new PagedResult<NewsArticleDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<NewsArticleDto>> GetUserNewsArticlesAsync(
        Guid userId,
        QueryParameters parameters,
        Guid? currentUserId = null)
    {
        var query = _context.Set<NewsArticle>()
            .Include(a => a.Author)
            .Where(a => a.AuthorId == userId)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = new List<NewsArticleDto>();
        foreach (var article in items)
        {
            dtos.Add(await MapToDtoAsync(article, currentUserId));
        }

        return new PagedResult<NewsArticleDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<List<NewsArticleDto>> GetFeaturedNewsAsync(int count = 5)
    {
        var articles = await _context.Set<NewsArticle>()
            .Include(a => a.Author)
            .Where(a => a.Status == NewsStatus.Published && a.IsFeatured)
            .OrderByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Take(count)
            .ToListAsync();

        var dtos = new List<NewsArticleDto>();
        foreach (var article in articles)
        {
            dtos.Add(await MapToDtoAsync(article, null));
        }

        return dtos;
    }

    public async Task<List<NewsArticleDto>> GetLatestNewsAsync(int count = 10)
    {
        var articles = await _context.Set<NewsArticle>()
            .Include(a => a.Author)
            .Where(a => a.Status == NewsStatus.Published)
            .OrderByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Take(count)
            .ToListAsync();

        var dtos = new List<NewsArticleDto>();
        foreach (var article in articles)
        {
            dtos.Add(await MapToDtoAsync(article, null));
        }

        return dtos;
    }

    public async Task PublishNewsArticleAsync(Guid articleId)
    {
        var article = await _context.Set<NewsArticle>()
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            throw new NotFoundException("News article not found");

        article.Publish();
        await _context.SaveChangesAsync();

        _logger.LogInformation("News article published: {ArticleId}", articleId);
    }

    public async Task ArchiveNewsArticleAsync(Guid articleId)
    {
        var article = await _context.Set<NewsArticle>()
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            throw new NotFoundException("News article not found");

        article.Archive();
        await _context.SaveChangesAsync();

        _logger.LogInformation("News article archived: {ArticleId}", articleId);
    }

    public async Task FeatureNewsArticleAsync(Guid articleId)
    {
        var article = await _context.Set<NewsArticle>()
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            throw new NotFoundException("News article not found");

        article.Feature();
        await _context.SaveChangesAsync();

        _logger.LogInformation("News article featured: {ArticleId}", articleId);
    }

    public async Task UnfeatureNewsArticleAsync(Guid articleId)
    {
        var article = await _context.Set<NewsArticle>()
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            throw new NotFoundException("News article not found");

        article.Unfeature();
        await _context.SaveChangesAsync();

        _logger.LogInformation("News article unfeatured: {ArticleId}", articleId);
    }

    public async Task IncrementViewsAsync(Guid articleId)
    {
        var article = await _context.Set<NewsArticle>()
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            throw new NotFoundException("News article not found");

        article.IncrementViews();
        await _context.SaveChangesAsync();
    }

    public async Task ToggleLikeAsync(Guid articleId, Guid userId)
    {
        var article = await _context.Set<NewsArticle>()
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            throw new NotFoundException("News article not found");

        // In a real implementation, track likes in a separate table
        article.IncrementLikes();
        await _context.SaveChangesAsync();

        _logger.LogInformation("News article {ArticleId} liked by user {UserId}", articleId, userId);
    }

    public async Task IncrementSharesAsync(Guid articleId)
    {
        var article = await _context.Set<NewsArticle>()
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            throw new NotFoundException("News article not found");

        article.IncrementShares();
        await _context.SaveChangesAsync();
    }

    public async Task<NewsComment> AddCommentAsync(Guid articleId, Guid userId, string content)
    {
        var comment = new NewsComment(articleId, userId, content);
        
        _context.Set<NewsComment>().Add(comment);
        
        // Increment comment count on article
        var article = await _context.Set<NewsArticle>()
            .FirstOrDefaultAsync(a => a.Id == articleId);
        if (article != null)
        {
            article.IncrementComments();
        }
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Comment added to news article {ArticleId} by user {UserId}", articleId, userId);
        return comment;
    }

    public async Task<NewsComment> UpdateCommentAsync(Guid commentId, Guid userId, string content)
    {
        var comment = await _context.Set<NewsComment>()
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new NotFoundException("Comment not found");

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only edit your own comments");

        comment.Update(content);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} updated", commentId);
        return comment;
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _context.Set<NewsComment>()
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new NotFoundException("Comment not found");

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only delete your own comments");

        // Decrement comment count on article
        var article = await _context.Set<NewsArticle>()
            .FirstOrDefaultAsync(a => a.Id == comment.NewsArticleId);
        if (article != null)
        {
            article.DecrementComments();
        }

        _context.Set<NewsComment>().Remove(comment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} deleted", commentId);
    }

    public async Task<PagedResult<NewsCommentDto>> GetNewsCommentsAsync(
        Guid articleId,
        QueryParameters parameters,
        Guid? currentUserId = null)
    {
        var query = _context.Set<NewsComment>()
            .Include(c => c.User)
            .Where(c => c.NewsArticleId == articleId)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(c => new NewsCommentDto
        {
            Id = c.Id,
            NewsArticleId = c.NewsArticleId,
            UserId = c.UserId,
            UserName = c.User.UserName ?? "Unknown",
            UserAvatar = c.User.ProfilePictureUrl,
            Content = c.Content,
            LikeCount = c.LikeCount,
            IsAuthor = currentUserId.HasValue && c.UserId == currentUserId.Value,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.ModifiedAt
        }).ToList();

        return new PagedResult<NewsCommentDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    private async Task<NewsArticleDto> MapToDtoAsync(NewsArticle article, Guid? currentUserId)
    {
        // In a real implementation, check if user has liked
        var isLiked = false;

        return new NewsArticleDto
        {
            Id = article.Id,
            Slug = article.Slug,
            Title = article.Title,
            Content = article.Content,
            Summary = article.Summary,
            ImageUrl = article.ImageUrl,
            Status = article.Status,
            StatusName = article.Status.ToString(),
            Category = article.Category,
            CategoryName = article.Category.ToString(),
            AuthorId = article.AuthorId,
            AuthorName = article.Author?.UserName ?? "Unknown",
            AuthorAvatar = article.Author?.ProfilePictureUrl,
            Source = article.Source,
            ExternalUrl = article.ExternalUrl,
            Tags = article.Tags,
            IsFeatured = article.IsFeatured,
            PublishedAt = article.PublishedAt,
            ViewCount = article.ViewCount,
            LikeCount = article.LikeCount,
            ShareCount = article.ShareCount,
            CommentCount = article.CommentCount,
            IsAuthor = currentUserId.HasValue && article.AuthorId == currentUserId.Value,
            IsLiked = isLiked,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.ModifiedAt
        };
    }
}
