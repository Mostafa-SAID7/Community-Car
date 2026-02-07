using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.maps;

public class MapPointComment : BaseEntity
{
    public Guid MapPointId { get; set; }
    public virtual MapPoint MapPoint { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
    
    public string Content { get; set; } = string.Empty;
    public int LikeCount { get; set; }

    private MapPointComment() { }

    public MapPointComment(Guid mapPointId, Guid userId, string content)
    {
        Guard.Against.Empty(mapPointId, nameof(mapPointId));
        Guard.Against.Empty(userId, nameof(userId));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));

        MapPointId = mapPointId;
        UserId = userId;
        Content = content;
    }

    public void Update(string content)
    {
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Content = content;
    }

    public void IncrementLikes() => LikeCount++;
    public void DecrementLikes() => LikeCount = Math.Max(0, LikeCount - 1);
}
