using CommunityCar.Domain.Enums.Community.qa;

namespace CommunityCar.Domain.Models;

public record ReactionSummary(
    ReactionType Type,
    int Count,
    bool HasUserReacted
);
