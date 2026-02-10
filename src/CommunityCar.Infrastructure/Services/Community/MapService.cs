using AutoMapper;
using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Entities.Community.maps;
using CommunityCar.Domain.Enums.Community.maps;
using CommunityCar.Domain.Exceptions;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Domain.Interfaces.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

public class MapService : IMapService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<MapService> _logger;

    public MapService(
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<MapService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<MapPoint> CreateMapPointAsync(string name, double latitude, double longitude, string? address, MapPointType type, Guid? ownerId = null, string? description = null)
    {
        var location = new Location(latitude, longitude, address);
        var mapPoint = new MapPoint(name, location, type, ownerId, description);
        
        // Check for duplicate slug and make it unique if necessary
        var baseSlug = mapPoint.Slug;
        var slug = baseSlug;
        var counter = 1;
        
        while (await _uow.Repository<MapPoint>().GetQueryable().AnyAsync(m => m.Slug == slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }
        
        // Update the slug if it was modified
        if (slug != baseSlug)
        {
            mapPoint.Slug = slug;
        }
        
        // Auto-publish the map point so it appears in the index immediately
        mapPoint.Publish();
        
        await _uow.Repository<MapPoint>().AddAsync(mapPoint);
        await _uow.SaveChangesAsync();
        _logger.LogInformation("Map point created and published: {MapPointId}", mapPoint.Id);
        return mapPoint;
    }

    public async Task<MapPoint> UpdateMapPointAsync(Guid mapPointId, string name, double latitude, double longitude, string? address, MapPointType type, string? description)
    {
        var mapPoint = await _uow.Repository<MapPoint>().GetQueryable().FirstOrDefaultAsync(m => m.Id == mapPointId);
        if (mapPoint == null) throw new NotFoundException("Map point not found");
        
        var location = new Location(latitude, longitude, address);
        mapPoint.Update(name, location, type, description);
        
        // Check for duplicate slug and make it unique if necessary (excluding current map point)
        var baseSlug = mapPoint.Slug;
        var slug = baseSlug;
        var counter = 1;
        
        while (await _uow.Repository<MapPoint>().GetQueryable().AnyAsync(m => m.Slug == slug && m.Id != mapPointId))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }
        
        // Update the slug if it was modified
        if (slug != baseSlug)
        {
            mapPoint.Slug = slug;
        }
        
        await _uow.SaveChangesAsync();
        _logger.LogInformation("Map point updated: {MapPointId}", mapPointId);
        return mapPoint;
    }

    public async Task DeleteMapPointAsync(Guid mapPointId)
    {
        var mapPoint = await _uow.Repository<MapPoint>().GetQueryable().FirstOrDefaultAsync(m => m.Id == mapPointId);
        if (mapPoint == null) throw new NotFoundException("Map point not found");
        
        _uow.Repository<MapPoint>().Delete(mapPoint);
        await _uow.SaveChangesAsync();
        _logger.LogInformation("Map point deleted: {MapPointId}", mapPointId);
    }

    public async Task<MapPointDto?> GetMapPointByIdAsync(Guid mapPointId, Guid? currentUserId = null)
    {
        var mapPoint = await _uow.Repository<MapPoint>().GetQueryable()
            .Include(m => m.Owner)
            .FirstOrDefaultAsync(m => m.Id == mapPointId);
        
        if (mapPoint == null) return null;
        
        var dto = _mapper.Map<MapPointDto>(mapPoint);
        if (currentUserId.HasValue)
        {
            dto.IsOwner = mapPoint.OwnerId == currentUserId.Value;
            dto.IsFavorited = await _uow.Repository<MapPointFavorite>().GetQueryable()
                .AnyAsync(f => f.MapPointId == mapPointId && f.UserId == currentUserId.Value);
        }
        return dto;
    }

    public async Task<MapPointDto?> GetMapPointBySlugAsync(string slug, Guid? currentUserId = null)
    {
        var mapPoint = await _uow.Repository<MapPoint>().GetQueryable()
            .Include(m => m.Owner)
            .FirstOrDefaultAsync(m => m.Slug == slug);
        
        if (mapPoint == null) return null;
        
        var dto = _mapper.Map<MapPointDto>(mapPoint);
        if (currentUserId.HasValue)
        {
            dto.IsOwner = mapPoint.OwnerId == currentUserId.Value;
            dto.IsFavorited = await _uow.Repository<MapPointFavorite>().GetQueryable()
                .AnyAsync(f => f.MapPointId == mapPoint.Id && f.UserId == currentUserId.Value);
        }
        return dto;
    }

    public async Task<PagedResult<MapPointDto>> GetMapPointsAsync(QueryParameters parameters, MapPointStatus? status = null, MapPointType? type = null, Guid? ownerId = null, Guid? currentUserId = null)
    {
        var query = _uow.Repository<MapPoint>().GetQueryable()
            .Include(m => m.Owner)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);
        if (type.HasValue)
            query = query.Where(m => m.Type == type.Value);
        if (ownerId.HasValue)
            query = query.Where(m => m.OwnerId == ownerId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<MapPointDto>>(items);
        
        if (currentUserId.HasValue)
        {
            var favoriteIds = await _uow.Repository<MapPointFavorite>().GetQueryable()
                .Where(f => f.UserId == currentUserId.Value && items.Select(i => i.Id).Contains(f.MapPointId))
                .Select(f => f.MapPointId)
                .ToListAsync();
            
            foreach (var dto in dtos)
            {
                dto.IsOwner = items.First(i => i.Id == dto.Id).OwnerId == currentUserId.Value;
                dto.IsFavorited = favoriteIds.Contains(dto.Id);
            }
        }

        return new PagedResult<MapPointDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public async Task<List<MapPointDto>> GetNearbyMapPointsAsync(double latitude, double longitude, double radiusKm = 10, MapPointType? type = null, Guid? currentUserId = null)
    {
        var query = _uow.Repository<MapPoint>().GetQueryable()
            .Include(m => m.Owner)
            .Where(m => m.Status == MapPointStatus.Published);

        if (type.HasValue)
            query = query.Where(m => m.Type == type.Value);

        var allPoints = await query.ToListAsync();
        
        var nearbyPoints = allPoints
            .Where(m => CalculateDistance(latitude, longitude, m.Location.Latitude, m.Location.Longitude) <= radiusKm)
            .OrderBy(m => CalculateDistance(latitude, longitude, m.Location.Latitude, m.Location.Longitude))
            .ToList();

        return _mapper.Map<List<MapPointDto>>(nearbyPoints);
    }

    public async Task<List<MapPointDto>> GetFeaturedMapPointsAsync(int count = 10)
    {
        var mapPoints = await _uow.Repository<MapPoint>().GetQueryable()
            .Include(m => m.Owner)
            .Where(m => m.Status == MapPointStatus.Published)
            .OrderByDescending(m => m.ViewCount)
            .Take(count)
            .ToListAsync();

        return _mapper.Map<List<MapPointDto>>(mapPoints);
    }

    public async Task<PagedResult<MapPointDto>> SearchMapPointsAsync(string searchTerm, QueryParameters parameters, Guid? currentUserId = null)
    {
        var query = _uow.Repository<MapPoint>().GetQueryable()
            .Include(m => m.Owner)
            .Where(m => m.Status == MapPointStatus.Published &&
                       (m.Name.Contains(searchTerm) || 
                        (m.Description != null && m.Description.Contains(searchTerm)) ||
                        (m.Location.Address != null && m.Location.Address.Contains(searchTerm))));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.ViewCount)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return _mapper.Map<PagedResult<MapPointDto>>(new PagedResult<MapPoint>(items, totalCount, parameters.PageNumber, parameters.PageSize));
    }

    public async Task PublishMapPointAsync(Guid mapPointId)
    {
        var mapPoint = await _uow.Repository<MapPoint>().GetQueryable().FirstOrDefaultAsync(m => m.Id == mapPointId);
        if (mapPoint == null) throw new NotFoundException("Map point not found");
        
        mapPoint.Publish();
        await _uow.SaveChangesAsync();
    }

    public async Task ArchiveMapPointAsync(Guid mapPointId)
    {
        var mapPoint = await _uow.Repository<MapPoint>().GetQueryable().FirstOrDefaultAsync(m => m.Id == mapPointId);
        if (mapPoint == null) throw new NotFoundException("Map point not found");
        
        mapPoint.Archive();
        await _uow.SaveChangesAsync();
    }

    public async Task VerifyMapPointAsync(Guid mapPointId)
    {
        var mapPoint = await _uow.Repository<MapPoint>().GetQueryable().FirstOrDefaultAsync(m => m.Id == mapPointId);
        if (mapPoint == null) throw new NotFoundException("Map point not found");
        
        mapPoint.Verify();
        await _uow.SaveChangesAsync();
    }

    public async Task UnverifyMapPointAsync(Guid mapPointId)
    {
        var mapPoint = await _uow.Repository<MapPoint>().GetQueryable().FirstOrDefaultAsync(m => m.Id == mapPointId);
        if (mapPoint == null) throw new NotFoundException("Map point not found");
        
        mapPoint.Unverify();
        await _uow.SaveChangesAsync();
    }

    public async Task IncrementViewsAsync(Guid mapPointId)
    {
        var mapPoint = await _uow.Repository<MapPoint>().GetQueryable().FirstOrDefaultAsync(m => m.Id == mapPointId);
        if (mapPoint == null) return;
        
        mapPoint.IncrementViews();
        await _uow.SaveChangesAsync();
    }

    public async Task ToggleFavoriteAsync(Guid mapPointId, Guid userId)
    {
        var favorite = await _uow.Repository<MapPointFavorite>().GetQueryable()
            .FirstOrDefaultAsync(f => f.MapPointId == mapPointId && f.UserId == userId);

        var mapPoint = await _uow.Repository<MapPoint>().GetQueryable().FirstOrDefaultAsync(m => m.Id == mapPointId);
        
        if (favorite == null)
        {
            await _uow.Repository<MapPointFavorite>().AddAsync(new MapPointFavorite(mapPointId, userId));
            if (mapPoint != null) mapPoint.IncrementFavorites();
        }
        else
        {
            _uow.Repository<MapPointFavorite>().Delete(favorite);
            if (mapPoint != null) mapPoint.DecrementFavorites();
        }

        await _uow.SaveChangesAsync();
    }

    public async Task CheckInAsync(Guid mapPointId, Guid userId)
    {
        var checkIn = new MapPointCheckIn(mapPointId, userId);
        await _uow.Repository<MapPointCheckIn>().AddAsync(checkIn);
        
        var mapPoint = await _uow.Repository<MapPoint>().GetQueryable().FirstOrDefaultAsync(m => m.Id == mapPointId);
        if (mapPoint != null)
        {
            mapPoint.IncrementCheckIns();
        }
        
        await _uow.SaveChangesAsync();
    }

    public async Task<MapPointRating> AddOrUpdateRatingAsync(Guid mapPointId, Guid userId, int rating, string? comment = null)
    {
        var existingRating = await _uow.Repository<MapPointRating>().GetQueryable()
            .FirstOrDefaultAsync(r => r.MapPointId == mapPointId && r.UserId == userId);

        if (existingRating != null)
        {
            existingRating.Update(rating, comment);
        }
        else
        {
            existingRating = new MapPointRating(mapPointId, userId, rating, comment);
            await _uow.Repository<MapPointRating>().AddAsync(existingRating);
        }

        await _uow.SaveChangesAsync();
        await UpdateMapPointAverageRatingAsync(mapPointId);
        return existingRating;
    }

    public async Task DeleteRatingAsync(Guid mapPointId, Guid userId)
    {
        var rating = await _uow.Repository<MapPointRating>().GetQueryable()
            .FirstOrDefaultAsync(r => r.MapPointId == mapPointId && r.UserId == userId);

        if (rating != null)
        {
            _uow.Repository<MapPointRating>().Delete(rating);
            await _uow.SaveChangesAsync();
            await UpdateMapPointAverageRatingAsync(mapPointId);
        }
    }

    public async Task<int?> GetUserRatingAsync(Guid mapPointId, Guid userId)
    {
        var rating = await _uow.Repository<MapPointRating>().GetQueryable()
            .FirstOrDefaultAsync(r => r.MapPointId == mapPointId && r.UserId == userId);
        return rating?.Rating;
    }

    public async Task<MapPointComment> AddCommentAsync(Guid mapPointId, Guid userId, string content)
    {
        var comment = new MapPointComment(mapPointId, userId, content);
        await _uow.Repository<MapPointComment>().AddAsync(comment);
        await _uow.SaveChangesAsync();
        return comment;
    }

    public async Task<MapPointComment> UpdateCommentAsync(Guid commentId, Guid userId, string content)
    {
        var comment = await _uow.Repository<MapPointComment>().GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);
        
        if (comment == null) throw new NotFoundException("Comment not found");
        
        comment.Update(content);
        await _uow.SaveChangesAsync();
        return comment;
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _uow.Repository<MapPointComment>().GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);
        
        if (comment != null)
        {
            _uow.Repository<MapPointComment>().Delete(comment);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task<PagedResult<MapPointCommentDto>> GetMapPointCommentsAsync(Guid mapPointId, QueryParameters parameters, Guid? currentUserId = null)
    {
        var query = _uow.Repository<MapPointComment>().GetQueryable()
            .Include(c => c.User)
            .Where(c => c.MapPointId == mapPointId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<MapPointCommentDto>>(items);
        return new PagedResult<MapPointCommentDto>(dtos, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    private async Task UpdateMapPointAverageRatingAsync(Guid mapPointId)
    {
        var ratings = await _uow.Repository<MapPointRating>().GetQueryable()
            .Where(r => r.MapPointId == mapPointId)
            .ToListAsync();

        var mapPoint = await _uow.Repository<MapPoint>().GetQueryable().FirstOrDefaultAsync(m => m.Id == mapPointId);
        if (mapPoint != null)
        {
            if (ratings.Any())
            {
                var averageRating = ratings.Average(r => r.Rating);
                mapPoint.UpdateRating(averageRating, ratings.Count);
            }
            else
            {
                mapPoint.UpdateRating(0, 0);
            }
            await _uow.SaveChangesAsync();
        }
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}

