using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Domain.Entities.Community.guides;

public class GuideLike : BaseEntity
{
    public Guid GuideId { get; set; }
    public virtual Guide Guide { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
}
