using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.maps;

public class MapPointCheckIn : BaseEntity
{
    public Guid MapPointId { get; set; }
    public virtual MapPoint MapPoint { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
    
    public DateTimeOffset CheckInTime { get; set; }
    public string? Note { get; set; }

    private MapPointCheckIn() { }

    public MapPointCheckIn(Guid mapPointId, Guid userId, string? note = null)
    {
        Guard.Against.Empty(mapPointId, nameof(mapPointId));
        Guard.Against.Empty(userId, nameof(userId));

        MapPointId = mapPointId;
        UserId = userId;
        CheckInTime = DateTimeOffset.UtcNow;
        Note = note;
    }
}
