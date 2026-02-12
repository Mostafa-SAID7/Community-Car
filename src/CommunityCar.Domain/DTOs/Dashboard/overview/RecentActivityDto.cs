namespace CommunityCar.Domain.DTOs.Dashboard.Overview;

public record RecentActivityDto(
    DateTime Timestamp,
    string UserName,
    string Action,
    string Status,
    string ActivityType
);
