using AutoMapper;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.guides;
using CommunityCar.Domain.Enums.Community.guides;
using CommunityCar.Domain.Enums.Community.qa;
using CommunityCar.Domain.Exceptions;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

public class GuideService : IGuideService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<GuideService> _logger;

    public GuideService(
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<GuideService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Guide> CreateGuideAsync(
        string title,
        string content,
        string summary,
        string category,
        Guid authorId,
        GuideDifficulty difficulty,
        int estimatedTimeMinutes)
    {
        var guide = new Guide(title, content, summary, category, authorId, difficulty, estimatedTimeMinutes);
        
        // Ensure slug is unique
        var baseSlug = guide.Slug;
        var slug = baseSlug;
        var counter = 1;
        
        while (await _uow.Repository<Guide>().GetQueryable().AnyAsync(g => g.Slug == slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }
        
        if (slug != baseSlug)
        {
            // Use reflection to set the slug since it's in BaseEntity
            var slugProperty = typeof(Guide).BaseType?.GetProperty("Slug");
            slugProperty?.SetValue(guide, slug);
        }
        
        await _uow.Repository<Guide>().AddAsync(guide);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Guide created: {GuideId} by user {UserId}", guide.Id, authorId);
        return guide;
    }

    public async Task<Guide> UpdateGuideAsync(
        Guid guideId,
        string title,
        string content,
        string summary,
        string category,
        GuideDifficulty difficulty,
        int estimatedTimeMinutes)
    {
        var guide = await _uow.Repository<Guide>()
            .FirstOrDefaultAsync(g => g.Id == guideId);

        if (guide == null)
            throw new NotFoundException("Guide not found");

        // Store the old slug to check if title changed
        var oldSlug = guide.Slug;
        
        guide.Update(title, content, summary, category, difficulty, estimatedTimeMinutes);
        
        // If slug changed, check for duplicates and make it unique
        if (guide.Slug != oldSlug)
        {
            var baseSlug = guide.Slug;
            var slug = baseSlug;
            var counter = 1;
            
            // Check if the new slug already exists (excluding current guide)
            while (await _uow.Repository<Guide>().GetQueryable()
                .AnyAsync(g => g.Slug == slug && g.Id != guideId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }
            
            // Update slug if it was changed
            if (slug != baseSlug)
            {
                guide.GetType().GetProperty("Slug")!.SetValue(guide, slug);
            }
        }
        
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Guide updated: {GuideId}", guideId);
        return guide;
    }

    public async Task DeleteGuideAsync(Guid guideId)
    {
        var guide = await _uow.Repository<Guide>()
            .FirstOrDefaultAsync(g => g.Id == guideId);

        if (guide == null)
            throw new NotFoundException("Guide not found");

        _uow.Repository<Guide>().Delete(guide);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Guide deleted: {GuideId}", guideId);
    }

    public async Task<GuideDto?> GetGuideByIdAsync(Guid guideId, Guid? currentUserId = null)
    {
        var query = _uow.Repository<Guide>().GetQueryable();
        var guide = await query
            .Include(g => g.Author)
            .FirstOrDefaultAsync(g => g.Id == guideId);

        if (guide == null)
            return null;

        return await MapToDtoAsync(guide, currentUserId);
    }

    public async Task<GuideDto?> GetGuideBySlugAsync(string slug, Guid? currentUserId = null)
    {
        var query = _uow.Repository<Guide>().GetQueryable();
        var guide = await query
            .Include(g => g.Author)
            .FirstOrDefaultAsync(g => g.Slug == slug);

        if (guide == null)
            return null;

        return await MapToDtoAsync(guide, currentUserId);
    }

    public async Task<PagedResult<GuideDto>> GetGuidesAsync(
        QueryParameters parameters,
        GuideStatus? status = null,
        GuideDifficulty? difficulty = null,
        string? category = null,
        Guid? currentUserId = null)
    {
        IQueryable<Guide> query = _uow.Repository<Guide>().GetQueryable()
            .Include(g => g.Author);

        if (status.HasValue)
            query = query.Where(g => g.Status == status.Value);
        else
            query = query.Where(g => g.Status == GuideStatus.Published);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(g => g.Category == category);

        if (difficulty.HasValue)
            query = query.Where(g => g.Difficulty == difficulty.Value);

        query = query.OrderByDescending(g => g.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = new List<GuideDto>();
        foreach (var guide in items)
        {
            dtos.Add(await MapToDtoAsync(guide, currentUserId));
        }

        return new PagedResult<GuideDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<PagedResult<GuideDto>> GetUserGuidesAsync(
        Guid userId,
        QueryParameters parameters,
        Guid? currentUserId = null)
    {
        var query = _uow.Repository<Guide>().GetQueryable()
            .Include(g => g.Author)
            .Where(g => g.AuthorId == userId)
            .OrderByDescending(g => g.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = new List<GuideDto>();
        foreach (var guide in items)
        {
            dtos.Add(await MapToDtoAsync(guide, currentUserId));
        }

        return new PagedResult<GuideDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        // Predefined categories
        var predefinedCategories = new List<string>
        {
            "Maintenance",
            "Repair",
            "Modification",
            "Detailing",
            "Performance",
            "Electrical",
            "Engine",
            "Transmission",
            "Suspension",
            "Brakes",
            "Interior",
            "Exterior",
            "Audio & Electronics",
            "Diagnostics",
            "Tools & Equipment",
            "Safety",
            "Fuel System",
            "Cooling System",
            "Exhaust",
            "Wheels & Tires"
        };

        // Get categories from published guides
        var usedCategories = await _uow.Repository<Guide>().GetQueryable()
            .Where(g => g.Status == GuideStatus.Published)
            .Select(g => g.Category)
            .Distinct()
            .ToListAsync();

        // Combine and remove duplicates
        var allCategories = predefinedCategories
            .Union(usedCategories)
            .OrderBy(c => c)
            .ToList();

        return allCategories;
    }

    public async Task<Dictionary<string, int>> GetCategoryCountsAsync()
    {
        return await _uow.Repository<Guide>().GetQueryable()
            .Where(g => g.Status == GuideStatus.Published)
            .GroupBy(g => g.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Category, x => x.Count);
    }

    public async Task<PagedResult<GuideDto>> SearchGuidesAsync(
        string searchTerm,
        QueryParameters parameters,
        Guid? currentUserId = null)
    {
        var query = _uow.Repository<Guide>().GetQueryable()
            .Include(g => g.Author)
            .Where(g => g.Status == GuideStatus.Published &&
                       (g.Title.Contains(searchTerm) ||
                        g.Summary.Contains(searchTerm) ||
                        g.Content.Contains(searchTerm) ||
                        g.Category.Contains(searchTerm)))
            .OrderByDescending(g => g.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = new List<GuideDto>();
        foreach (var guide in items)
        {
            dtos.Add(await MapToDtoAsync(guide, currentUserId));
        }

        return new PagedResult<GuideDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task PublishGuideAsync(Guid guideId)
    {
        var guide = await _uow.Repository<Guide>()
            .FirstOrDefaultAsync(g => g.Id == guideId);

        if (guide == null)
            throw new NotFoundException("Guide not found");

        guide.Publish();
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Guide published: {GuideId}", guideId);
    }

    public async Task ArchiveGuideAsync(Guid guideId)
    {
        var guide = await _uow.Repository<Guide>()
            .FirstOrDefaultAsync(g => g.Id == guideId);

        if (guide == null)
            throw new NotFoundException("Guide not found");

        guide.Archive();
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Guide archived: {GuideId}", guideId);
    }

    public async Task SubmitForReviewAsync(Guid guideId)
    {
        var guide = await _uow.Repository<Guide>()
            .FirstOrDefaultAsync(g => g.Id == guideId);

        if (guide == null)
            throw new NotFoundException("Guide not found");

        guide.SubmitForReview();
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Guide submitted for review: {GuideId}", guideId);
    }

    public async Task IncrementViewsAsync(Guid guideId)
    {
        var guide = await _uow.Repository<Guide>()
            .FirstOrDefaultAsync(g => g.Id == guideId);

        if (guide == null)
            throw new NotFoundException("Guide not found");

        guide.IncrementViews();
        await _uow.SaveChangesAsync();
    }

    public async Task IncrementViewCountAsync(Guid guideId)
    {
        await IncrementViewsAsync(guideId);
    }

    public async Task ToggleLikeAsync(Guid guideId, Guid userId)
        {
            var guide = await _uow.Repository<Guide>()
                .FirstOrDefaultAsync(g => g.Id == guideId);

            if (guide == null)
                throw new NotFoundException("Guide not found");

            var existingReaction = await _uow.Repository<GuideReaction>()
                .FirstOrDefaultAsync(r => r.GuideId == guideId && r.UserId == userId);

            if (existingReaction != null)
            {
                // Unlike - remove the reaction
                _uow.Repository<GuideReaction>().Delete(existingReaction);
                guide.DecrementLikes();
            }
            else
            {
                // Like - add new reaction
                var reaction = new GuideReaction(guideId, userId, ReactionType.Like);
                await _uow.Repository<GuideReaction>().AddAsync(reaction);
                guide.IncrementLikes();
            }

            await _uow.SaveChangesAsync();

            _logger.LogInformation("Guide {GuideId} like toggled by user {UserId}", guideId, userId);
        }


    public async Task ToggleBookmarkAsync(Guid guideId, Guid userId)
    {
        var guide = await _uow.Repository<Guide>()
            .FirstOrDefaultAsync(g => g.Id == guideId);

        if (guide == null)
            throw new NotFoundException("Guide not found");

        var existingBookmark = await _uow.Repository<GuideBookmark>()
            .FirstOrDefaultAsync(gb => gb.GuideId == guideId && gb.UserId == userId);

        if (existingBookmark != null)
        {
            // Remove bookmark
            _uow.Repository<GuideBookmark>().Delete(existingBookmark);
            guide.DecrementBookmarks();
            _logger.LogInformation("Guide {GuideId} unbookmarked by user {UserId}", guideId, userId);
        }
        else
        {
            // Add bookmark
            var bookmark = new GuideBookmark(guideId, userId);
            await _uow.Repository<GuideBookmark>().AddAsync(bookmark);
            guide.IncrementBookmarks();
            _logger.LogInformation("Guide {GuideId} bookmarked by user {UserId}", guideId, userId);
        }

        await _uow.SaveChangesAsync();
    }

    public async Task<GuideStep> AddStepAsync(Guid guideId, int stepNumber, string title, string content, int estimatedTimeMinutes)
    {
        var step = new GuideStep(guideId, stepNumber, title, content, estimatedTimeMinutes);
        
        await _uow.Repository<GuideStep>().AddAsync(step);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Step added to guide {GuideId}", guideId);
        return step;
    }

    public async Task<GuideStep> UpdateStepAsync(Guid stepId, string title, string content, int estimatedTimeMinutes)
    {
        var step = await _uow.Repository<GuideStep>()
            .FirstOrDefaultAsync(s => s.Id == stepId);

        if (step == null)
            throw new NotFoundException("Step not found");

        step.Update(title, content, estimatedTimeMinutes);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Step {StepId} updated", stepId);
        return step;
    }

    public async Task DeleteStepAsync(Guid stepId)
    {
        var step = await _uow.Repository<GuideStep>()
            .FirstOrDefaultAsync(s => s.Id == stepId);

        if (step == null)
            throw new NotFoundException("Step not found");

        _uow.Repository<GuideStep>().Delete(step);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Step {StepId} deleted", stepId);
    }

    public async Task<List<GuideStepDto>> GetGuideStepsAsync(Guid guideId)
    {
        var steps = await _uow.Repository<GuideStep>().GetQueryable()
            .Where(s => s.GuideId == guideId)
            .OrderBy(s => s.StepNumber)
            .ToListAsync();

        return steps.Select(s => new GuideStepDto
        {
            Id = s.Id,
            GuideId = s.GuideId,
            StepNumber = s.StepNumber,
            Title = s.Title,
            Content = s.Content,
            ImageUrl = s.ImageUrl,
            VideoUrl = s.VideoUrl,
            EstimatedTimeMinutes = s.EstimatedTimeMinutes
        }).ToList();
    }

    public async Task<GuideComment> AddCommentAsync(Guid guideId, Guid userId, string content)
    {
        var comment = new GuideComment(guideId, userId, content);
        
        await _uow.Repository<GuideComment>().AddAsync(comment);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Comment added to guide {GuideId} by user {UserId}", guideId, userId);
        return comment;
    }

    public async Task<GuideComment> UpdateCommentAsync(Guid commentId, Guid userId, string content)
    {
        var comment = await _uow.Repository<GuideComment>()
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new NotFoundException("Comment not found");

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only edit your own comments");

        comment.Update(content);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} updated", commentId);
        return comment;
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _uow.Repository<GuideComment>()
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new NotFoundException("Comment not found");

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only delete your own comments");

        _uow.Repository<GuideComment>().Delete(comment);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("Comment {CommentId} deleted", commentId);
    }

    public async Task<PagedResult<GuideCommentDto>> GetGuideCommentsAsync(
        Guid guideId,
        QueryParameters parameters,
        Guid? currentUserId = null)
    {
        var query = _uow.Repository<GuideComment>().GetQueryable()
            .Include(c => c.User)
            .Where(c => c.GuideId == guideId)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = items.Select(c => new GuideCommentDto
        {
            Id = c.Id,
            GuideId = c.GuideId,
            UserId = c.UserId,
            UserName = c.User.UserName ?? "Unknown",
            UserAvatar = c.User.ProfilePictureUrl,
            Content = c.Content,
            LikeCount = c.LikeCount,
            IsAuthor = currentUserId.HasValue && c.UserId == currentUserId.Value,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.ModifiedAt
        }).ToList();

        return new PagedResult<GuideCommentDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    private async Task<GuideDto> MapToDtoAsync(Guide guide, Guid? currentUserId)
    {
        var isLiked = false;
        var isBookmarked = false;

        if (currentUserId.HasValue)
        {
            isLiked = await _uow.Repository<GuideReaction>().GetQueryable()
                .AnyAsync(gr => gr.GuideId == guide.Id && gr.UserId == currentUserId.Value);
            
            isBookmarked = await _uow.Repository<GuideBookmark>().GetQueryable()
                .AnyAsync(gb => gb.GuideId == guide.Id && gb.UserId == currentUserId.Value);
        }

        return new GuideDto
        {
            Id = guide.Id,
            Slug = guide.Slug,
            Title = guide.Title,
            Content = guide.Content,
            Summary = guide.Summary,
            Category = guide.Category,
            Status = guide.Status,
            StatusName = guide.Status.ToString(),
            Difficulty = guide.Difficulty,
            DifficultyName = guide.Difficulty.ToString(),
            AuthorId = guide.AuthorId,
            AuthorName = guide.Author?.UserName ?? "Unknown",
            AuthorAvatar = guide.Author?.ProfilePictureUrl,
            EstimatedTimeMinutes = guide.EstimatedTimeMinutes,
            Tags = guide.Tags,
            ImageUrl = guide.ImageUrl,
            VideoUrl = guide.VideoUrl,
            ViewCount = guide.ViewCount,
            LikeCount = guide.LikeCount,
            BookmarkCount = guide.BookmarkCount,
            AverageRating = guide.AverageRating,
            RatingCount = guide.RatingCount,
            IsAuthor = currentUserId.HasValue && guide.AuthorId == currentUserId.Value,
            IsLiked = isLiked,
            IsBookmarked = isBookmarked,
            CreatedAt = guide.CreatedAt,
            UpdatedAt = guide.ModifiedAt
        };
    }
}
