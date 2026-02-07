using CommunityCar.Domain.Base;
using CommunityCar.Domain.Enums.Gamification;

namespace CommunityCar.Domain.Entities.Gamification;

public class Badge : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public BadgeCategory Category { get; set; }
    public BadgeTier Tier { get; set; }
    public int PointsRequired { get; set; }
    public string RequirementDescription { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    
    // Navigation properties
    public virtual ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}
