using CommunityCar.Domain.Enums.Gamification;

namespace CommunityCar.Domain.DTOs.Gamification;

public class BadgeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public BadgeCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public BadgeTier Tier { get; set; }
    public string TierName { get; set; } = string.Empty;
    public int PointsRequired { get; set; }
    public string RequirementDescription { get; set; } = string.Empty;
    public bool IsEarned { get; set; }
    public DateTimeOffset? EarnedAt { get; set; }
    public int EarnedByCount { get; set; }
}
