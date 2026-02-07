using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Community.maps;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.maps;

public class MapPoint : AggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Location Location { get; set; } = default!;
    public MapPointType Type { get; set; }
    public MapPointStatus Status { get; set; }
    
    public Guid? OwnerId { get; set; }
    public virtual ApplicationUser? Owner { get; set; }
    
    // Additional Info
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public string? ImageUrl { get; set; }
    public string? Tags { get; set; } // JSON array
    
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
    
    // Navigation
    public virtual ICollection<MapPointComment> Comments { get; set; } = new List<MapPointComment>();
    public virtual ICollection<MapPointRating> Ratings { get; set; } = new List<MapPointRating>();

    private MapPoint() { }

    public MapPoint(
        string name,
        Location location,
        MapPointType type,
        Guid? ownerId = null,
        string? description = null)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.Null(location, nameof(location));

        Name = name;
        Location = location;
        Type = type;
        OwnerId = ownerId;
        Description = description;
        Status = MapPointStatus.Draft;
        Slug = SlugHelper.GenerateSlug(name);
    }

    public void Update(string name, Location location, MapPointType type, string? description)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.Null(location, nameof(location));

        Name = name;
        Location = location;
        Type = type;
        Description = description;
        Slug = SlugHelper.GenerateSlug(name);
    }

    public void Publish()
    {
        Status = MapPointStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
    }

    public void Archive() => Status = MapPointStatus.Archived;
    
    public void Verify()
    {
        Status = MapPointStatus.Verified;
        IsVerified = true;
        VerifiedAt = DateTimeOffset.UtcNow;
    }

    public void Unverify()
    {
        IsVerified = false;
        VerifiedAt = null;
    }
    
    public void IncrementViews() => ViewCount++;
    public void IncrementFavorites() => FavoriteCount++;
    public void DecrementFavorites() => FavoriteCount = Math.Max(0, FavoriteCount - 1);
    public void IncrementCheckIns() => CheckInCount++;
    
    public void UpdateRating(double newRating, int newCount)
    {
        AverageRating = newRating;
        RatingCount = newCount;
    }
    
    public void SetContactInfo(string? phoneNumber, string? website)
    {
        PhoneNumber = phoneNumber;
        Website = website;
    }
    
    public void SetImage(string? imageUrl) => ImageUrl = imageUrl;
}
