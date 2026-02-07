using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Utilities;

namespace CommunityCar.Domain.Entities.Community.maps;

public class MapPointFavorite : BaseEntity
{
    public Guid MapPointId { get; set; }
    public virtual MapPoint MapPoint { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    private MapPointFavorite() { }

    public MapPointFavorite(Guid mapPointId, Guid userId)
    {
        Guard.Against.Empty(mapPointId, nameof(mapPointId));
        Guard.Against.Empty(userId, nameof(userId));

        MapPointId = mapPointId;
        UserId = userId;
    }
}
