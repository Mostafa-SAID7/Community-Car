using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.guides;
using CommunityCar.Domain.Enums.Community.guides;

namespace CommunityCar.Domain.Interfaces.Community;

public interface IGuideService
{
    // CRUD Operations
    Task<Guide> CreateGuideAsync(
        string title,
        string content,
        string summary,
        string category,
        Guid authorId,
        GuideDifficulty difficulty,
        int estimatedTimeMinutes);
    
    Task<Guide> UpdateGuideAsync(
        Guid guideId,
        string title,
        string content,
        string summary,
        string category,
        GuideDifficulty difficulty,
        int estimatedTimeMinutes);
    
    Task DeleteGuideAsync(Guid guideId);
    
    // Query Operations
    Task<GuideDto?> GetGuideByIdAsync(Guid guideId, Guid? currentUserId = null);
    Task<GuideDto?> GetGuideBySlugAsync(string slug, Guid? currentUserId = null);
    
    Task<PagedResult<GuideDto>> GetGuidesAsync(
        QueryParameters parameters,
        GuideStatus? status = null,
        GuideDifficulty? difficulty = null,
        string? category = null,
        Guid? currentUserId = null);
    
    Task<PagedResult<GuideDto>> GetUserGuidesAsync(
        Guid userId,
        QueryParameters parameters,
        Guid? currentUserId = null);
    
    Task<PagedResult<GuideDto>> SearchGuidesAsync(
        string searchTerm,
        QueryParameters parameters,
        Guid? currentUserId = null);
    
    // Status Management
    Task PublishGuideAsync(Guid guideId);
    Task ArchiveGuideAsync(Guid guideId);
    Task SubmitForReviewAsync(Guid guideId);
    
    // Engagement
    Task IncrementViewsAsync(Guid guideId);
    Task IncrementViewCountAsync(Guid guideId); // Alias
    Task ToggleLikeAsync(Guid guideId, Guid userId);
    Task ToggleBookmarkAsync(Guid guideId, Guid userId);
    
    // Statistics
    Task<List<string>> GetCategoriesAsync();
    Task<Dictionary<string, int>> GetCategoryCountsAsync();
}
