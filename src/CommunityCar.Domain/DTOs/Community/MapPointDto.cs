using CommunityCar.Domain.Enums.Community.maps;

namespace CommunityCar.Domain.DTOs.Community;

public class MapPointDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Location
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    
    public MapPointType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public MapPointStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    
    public Guid? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerAvatar { get; set; }
    
    // Additional Info
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public string? ImageUrl { get; set; }
    public string? Tags { get; set; }
    
    // Ratings & Engagement
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int ViewCount { get; set; }
    public int FavoriteCount { get; set; }
    public int CheckInCount { get; set; }
    
    // Metadata
    public bool IsVerified { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    
    // User context
    public bool IsOwner { get; set; }
    public bool IsFavorited { get; set; }
    public int? UserRating { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
