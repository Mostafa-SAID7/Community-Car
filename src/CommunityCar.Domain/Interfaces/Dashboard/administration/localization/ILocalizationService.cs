using CommunityCar.Domain.Base;
using CommunityCar.Domain.DTOs.Dashboard.Administration.Localization;

namespace CommunityCar.Domain.Interfaces.Dashboard.Administration.Localization;

public interface ILocalizationService
{
    Task<PagedResult<LocalizationResourceDto>> GetResourcesAsync(LocalizationFilterDto filter);
    Task<LocalizationResourceDto?> GetResourceByIdAsync(Guid id);
    Task<LocalizationResourceDto?> GetResourceByKeyAsync(string key, string cultureCode);
    Task<LocalizationResourceDto> CreateResourceAsync(CreateLocalizationResourceDto dto);
    Task<LocalizationResourceDto> UpdateResourceAsync(UpdateLocalizationResourceDto dto);
    Task DeleteResourceAsync(Guid id);
    Task<List<string>> GetDistinctCategoriesAsync();
    Task<List<string>> GetDistinctCulturesAsync();
    Task<LocalizationStatisticsDto> GetStatisticsAsync();
    Task<Dictionary<string, string>> GetAllResourcesForCultureAsync(string cultureCode);
    Task<int> BulkImportAsync(BulkImportDto dto);
    Task<Dictionary<string, Dictionary<string, string>>> ExportAllAsync();
    Task<List<LocalizationResourceDto>> GetMissingTranslationsAsync(string sourceCulture, string targetCulture);
    Task<int> SyncFromResxFilesAsync();
    Task<int> SyncToResxFilesAsync();
    Task<List<string>> GetAvailableCulturesAsync();
    Task<List<string>> GetAvailableCategoriesAsync();
    Task<Dictionary<string, string>> ExportResourcesAsync(string? culture, string? category);
    Task<int> SyncMissingKeysAsync(string sourceCulture, string targetCulture, bool overwriteExisting);
    Task RefreshCacheAsync();
    Task<Dictionary<string, string>> GetCachedResourcesAsync(string cultureCode);
}
