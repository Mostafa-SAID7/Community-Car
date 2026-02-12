namespace CommunityCar.Domain.DTOs.Dashboard;

public record RecentActivityDto(
    DateTime Timestamp,
    string UserName,
    string Action,
    string Status,
    string ActivityType
);
