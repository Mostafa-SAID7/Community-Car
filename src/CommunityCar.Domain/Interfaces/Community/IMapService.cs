using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.maps;
using CommunityCar.Domain.Enums.Community.maps;

namespace CommunityCar.Domain.Interfaces.Community;

public interface IMapService
{
    // CRUD Operations
    Task<MapPoint> CreateMapPointAsync(string name, double latitude, double longitude, string? address, MapPointType type, Guid? ownerId = null, string? description = null);
    Task<MapPoint> UpdateMapPointAsync(Guid mapPointId, string name, double latitude, double longitude, string? address, MapPointType type, string? description);
    Task DeleteMapPointAsync(Guid mapPointId);
    
    // Query Operations
    Task<MapPointDto?> GetMapPointByIdAsync(Guid mapPointId, Guid? currentUserId = null);
    Task<MapPointDto?> GetMapPointBySlugAsync(string slug, Guid? currentUserId = null);
    Task<PagedResult<MapPointDto>> GetMapPointsAsync(QueryParameters parameters, MapPointStatus? status = null, MapPointType? type = null, Guid? ownerId = null, Guid? currentUserId = null);
    Task<List<MapPointDto>> GetNearbyMapPointsAsync(double latitude, double longitude, double radiusKm = 10, MapPointType? type = null, Guid? currentUserId = null);
    Task<List<MapPointDto>> GetFeaturedMapPointsAsync(int count = 10);
    Task<PagedResult<MapPointDto>> SearchMapPointsAsync(string searchTerm, QueryParameters parameters, Guid? currentUserId = null);
    
    // Status Management
    Task PublishMapPointAsync(Guid mapPointId);
    Task ArchiveMapPointAsync(Guid mapPointId);
    Task VerifyMapPointAsync(Guid mapPointId);
    Task UnverifyMapPointAsync(Guid mapPointId);
    
    // Engagement
    Task IncrementViewsAsync(Guid mapPointId);
    Task ToggleFavoriteAsync(Guid mapPointId, Guid userId);
    Task CheckInAsync(Guid mapPointId, Guid userId);
    
    // Ratings
    Task<MapPointRating> AddOrUpdateRatingAsync(Guid mapPointId, Guid userId, int rating, string? comment = null);
    Task DeleteRatingAsync(Guid mapPointId, Guid userId);
    Task<int?> GetUserRatingAsync(Guid mapPointId, Guid userId);
    
    // Comments
    Task<MapPointComment> AddCommentAsync(Guid mapPointId, Guid userId, string content);
    Task<MapPointComment> UpdateCommentAsync(Guid commentId, Guid userId, string content);
    Task DeleteCommentAsync(Guid commentId, Guid userId);
    Task<PagedResult<MapPointCommentDto>> GetMapPointCommentsAsync(Guid mapPointId, QueryParameters parameters, Guid? currentUserId = null);
}
