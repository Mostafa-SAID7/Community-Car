using CommunityCar.Domain.Enums.Community.qa;

namespace CommunityCar.Domain.DTOs.Community;

public class ReactionSummaryDto
{
    public ReactionType ReactionType { get; set; }
    public int Count { get; set; }
    public bool UserReacted { get; set; }
}

public class ReactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public ReactionType ReactionType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
