using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.maps;

public class MapPointReview : BaseEntity
{
    public Guid MapPointId { get; set; }
    public virtual MapPoint MapPoint { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
    
    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
    public string? ImageUrl { get; set; }
    
    public int HelpfulCount { get; set; }

    private MapPointReview() { }

    public MapPointReview(Guid mapPointId, Guid userId, int rating, string? comment = null)
    {
        Guard.Against.Empty(mapPointId, nameof(mapPointId));
        Guard.Against.Empty(userId, nameof(userId));
        
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5");

        MapPointId = mapPointId;
        UserId = userId;
        Rating = rating;
        Comment = comment;
    }

    public void Update(int rating, string? comment)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5");
            
        Rating = rating;
        Comment = comment;
    }

    public void IncrementHelpful() => HelpfulCount++;
    public void DecrementHelpful() => HelpfulCount = Math.Max(0, HelpfulCount - 1);
}
