using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Entities.Dashboard.KPIs;

namespace CommunityCar.Domain.Interfaces.Dashboard;

public interface IKPIService
{
    Task<PagedResult<KPIDto>> GetKPIsAsync(KPIFilterDto filter);
    Task<KPIDto?> GetKPIByIdAsync(Guid id);
    Task<KPIDto?> GetKPIByCodeAsync(string code);
    Task<KPI> CreateKPIAsync(CreateKPIDto dto);
    Task<KPI> UpdateKPIAsync(UpdateKPIDto dto);
    Task UpdateKPIValueAsync(Guid id, double newValue);
    Task UpdateKPIValueByCodeAsync(string code, double newValue);
    Task SetKPITargetAsync(Guid id, double target);
    Task DeleteKPIAsync(Guid id);
    Task ActivateKPIAsync(Guid id);
    Task DeactivateKPIAsync(Guid id);
    Task<List<string>> GetCategoriesAsync();
    Task<KPIStatisticsDto> GetStatisticsAsync();
    Task<KPISummaryDto> GetKPISummaryAsync();
    Task<List<KPIDto>> GetKPIsByCategoryAsync(string category);
    Task<Dictionary<string, double>> GetKPITrendsAsync(string code, int days = 30);
}
