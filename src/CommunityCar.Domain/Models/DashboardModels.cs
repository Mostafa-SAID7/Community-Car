namespace CommunityCar.Domain.Models;

public record DashboardSummary(
    int TotalUsers,
    string? Slug,
    int TotalFriendships,
    int ActiveEvents,
    double SystemHealth,
    int TotalPosts,
    int TotalQuestions,
    int TotalGroups,
    int TotalReviews,
    int TotalGuides,
    int TotalNews,
    int ActiveUsersToday,
    int NewUsersThisWeek,
    int NewUsersThisMonth,
    double EngagementRate,
    double UserGrowthPercentage
);

public record KPIValue(string Label, double Value);
