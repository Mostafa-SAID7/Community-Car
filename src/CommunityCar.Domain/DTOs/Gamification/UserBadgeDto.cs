namespace CommunityCar.Domain.DTOs.Gamification;

public class UserBadgeDto
{
    public Guid Id { get; set; }
    public Guid BadgeId { get; set; }
    public string BadgeName { get; set; } = string.Empty;
    public string BadgeDescription { get; set; } = string.Empty;
    public string BadgeIconUrl { get; set; } = string.Empty;
    public string BadgeTier { get; set; } = string.Empty;
    public string BadgeCategory { get; set; } = string.Empty;
    public DateTimeOffset EarnedAt { get; set; }
    public string? EarnedReason { get; set; }
    public bool IsDisplayed { get; set; }
}
