using CommunityCar.Domain.Enums.Community.qa;

namespace CommunityCar.Domain.Models;

public record ReactionSummary(
    ReactionType ReactionType,
    int Count,
    bool IsUserReaction
);
