using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard;
using CommunityCar.Domain.Enums.Dashboard.settings;

namespace CommunityCar.Domain.Interfaces.Dashboard;

public interface ISystemSettingService
{
    Task<PagedResult<SystemSettingDto>> GetSettingsAsync(SystemSettingFilterDto filter);
    Task<SystemSettingDto?> GetSettingByIdAsync(Guid id);
    Task<SystemSettingDto?> GetSettingByKeyAsync(string key);
    Task<string?> GetSettingValueAsync(string key);
    Task<T?> GetSettingValueAsync<T>(string key);
    Task<SystemSettingDto> CreateSettingAsync(CreateSystemSettingDto dto);
    Task UpdateSettingAsync(UpdateSystemSettingDto dto);
    Task UpdateSettingValueAsync(string key, string value);
    Task DeleteSettingAsync(Guid id);
    Task ResetToDefaultAsync(Guid id);
    Task<SettingsSummaryDto> GetSummaryAsync();
    Task<Dictionary<string, SystemSettingDto>> GetSettingsByCategoryAsync(SettingCategory category);
    Task<bool> SettingExistsAsync(string key);
    Task BulkUpdateSettingsAsync(Dictionary<string, string> settings);
}
