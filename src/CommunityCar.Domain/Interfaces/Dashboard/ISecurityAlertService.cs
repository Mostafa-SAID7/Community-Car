using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Entities.Dashboard.security;

namespace CommunityCar.Domain.Interfaces.Dashboard;

public interface ISecurityAlertService
{
    // Main CRUD operations
    Task<PagedResult<SecurityAlertDto>> GetAlertsAsync(SecurityAlertFilterDto filter);
    Task<SecurityAlertDto?> GetAlertByIdAsync(Guid id);
    Task<SecurityAlert> CreateAlertAsync(CreateSecurityAlertDto dto);
    Task<SecurityAlert> UpdateAlertAsync(UpdateSecurityAlertDto dto);
    Task DeleteAlertAsync(Guid id);
    
    // Alert management
    Task ResolveAlertAsync(Guid id, Guid resolvedById, string resolvedByName, string? resolutionNotes = null);
    Task ReopenAlertAsync(Guid id);
    
    // Statistics and reporting
    Task<SecurityAlertStatisticsDto> GetStatisticsAsync(int days = 30);
    Task<SecuritySummaryDto> GetSummaryAsync();
    Task<List<SecurityAlertDto>> GetUnresolvedAlertsAsync(int count = 10);
    Task<List<SecurityAlertDto>> GetCriticalAlertsAsync();
    Task<List<SecurityAlertDto>> GetAlertsByUserAsync(Guid userId);
    Task<Dictionary<string, int>> GetAlertTrendsAsync(int days = 30);
}
