namespace CommunityCar.Domain.Models;

public record DashboardSummary(
    int TotalUsers,
    string? Slug,
    int TotalFriendships,
    int ActiveEvents,
    double SystemHealth
);

public record KPIValue(string Label, double Value);
