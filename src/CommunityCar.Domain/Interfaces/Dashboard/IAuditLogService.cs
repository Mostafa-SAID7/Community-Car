using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard;

namespace CommunityCar.Domain.Interfaces.Dashboard;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogFilterDto filter);
    Task<AuditLogDto?> GetAuditLogByIdAsync(Guid id);
    Task<List<string>> GetDistinctEntityNamesAsync();
    Task<List<string>> GetDistinctActionsAsync();
    Task<Dictionary<string, int>> GetAuditLogStatisticsAsync(int days);
}
