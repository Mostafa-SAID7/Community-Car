using CommunityCar.Domain.DTOs.Dashboard.Administration.Settings;

namespace CommunityCar.Domain.Interfaces.Dashboard.Administration.Settings;

public interface ISettingsService
{
    // Application Settings
    Task<ApplicationSettingsDto> GetApplicationSettingsAsync();
    Task UpdateApplicationSettingsAsync(UpdateApplicationSettingsDto dto);
    
    // Email Settings
    Task<EmailSettingsDto> GetEmailSettingsAsync();
    Task UpdateEmailSettingsAsync(UpdateEmailSettingsDto dto);
    Task<bool> TestEmailConnectionAsync();
    
    // Security Settings
    Task<SecuritySettingsDto> GetSecuritySettingsAsync();
    Task UpdateSecuritySettingsAsync(UpdateSecuritySettingsDto dto);
    
    // Notification Settings
    Task<NotificationSettingsDto> GetNotificationSettingsAsync();
    Task UpdateNotificationSettingsAsync(UpdateNotificationSettingsDto dto);
    
    // Generic Settings
    Task<string?> GetSettingAsync(string key);
    Task SetSettingAsync(string key, string value, string category, Guid? modifiedBy = null);
    Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category);
    
    // Cache Management
    Task ClearSettingsCacheAsync();
}
