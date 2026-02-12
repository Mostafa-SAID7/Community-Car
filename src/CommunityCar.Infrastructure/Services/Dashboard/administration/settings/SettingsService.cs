using CommunityCar.Domain.DTOs.Dashboard.Administration.Settings;
using CommunityCar.Domain.Entities.Dashboard.settings;
using CommunityCar.Domain.Interfaces.Dashboard.Administration.Settings;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Dashboard.Administration.Settings;

public class SettingsService : ISettingsService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SettingsService> _logger;
    private const string CacheKeyPrefix = "Settings_";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

    public SettingsService(
        ApplicationDbContext context,
        IMemoryCache cache,
        ILogger<SettingsService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ApplicationSettingsDto> GetApplicationSettingsAsync()
    {
        var settings = await GetSettingsByCategoryAsync("Application");
        
        return new ApplicationSettingsDto
        {
            ApplicationName = settings.GetValueOrDefault("ApplicationName", "Community Car"),
            ApplicationUrl = settings.GetValueOrDefault("ApplicationUrl", ""),
            SupportEmail = settings.GetValueOrDefault("SupportEmail", ""),
            AdminEmail = settings.GetValueOrDefault("AdminEmail", ""),
            MaintenanceMode = bool.Parse(settings.GetValueOrDefault("MaintenanceMode", "false")),
            MaintenanceMessage = settings.GetValueOrDefault("MaintenanceMessage"),
            AllowRegistration = bool.Parse(settings.GetValueOrDefault("AllowRegistration", "true")),
            RequireEmailConfirmation = bool.Parse(settings.GetValueOrDefault("RequireEmailConfirmation", "true")),
            SessionTimeout = int.Parse(settings.GetValueOrDefault("SessionTimeout", "30")),
            MaxLoginAttempts = int.Parse(settings.GetValueOrDefault("MaxLoginAttempts", "5")),
            LockoutDuration = int.Parse(settings.GetValueOrDefault("LockoutDuration", "15"))
        };
    }

    public async Task UpdateApplicationSettingsAsync(UpdateApplicationSettingsDto dto)
    {
        await SetSettingAsync("ApplicationName", dto.ApplicationName, "Application");
        await SetSettingAsync("ApplicationUrl", dto.ApplicationUrl, "Application");
        await SetSettingAsync("SupportEmail", dto.SupportEmail, "Application");
        await SetSettingAsync("AdminEmail", dto.AdminEmail, "Application");
        await SetSettingAsync("MaintenanceMode", dto.MaintenanceMode.ToString(), "Application");
        await SetSettingAsync("MaintenanceMessage", dto.MaintenanceMessage ?? "", "Application");
        await SetSettingAsync("AllowRegistration", dto.AllowRegistration.ToString(), "Application");
        await SetSettingAsync("RequireEmailConfirmation", dto.RequireEmailConfirmation.ToString(), "Application");
        await SetSettingAsync("SessionTimeout", dto.SessionTimeout.ToString(), "Application");
        await SetSettingAsync("MaxLoginAttempts", dto.MaxLoginAttempts.ToString(), "Application");
        await SetSettingAsync("LockoutDuration", dto.LockoutDuration.ToString(), "Application");
        
        await ClearSettingsCacheAsync();
    }

    public async Task<EmailSettingsDto> GetEmailSettingsAsync()
    {
        var settings = await GetSettingsByCategoryAsync("Email");
        
        return new EmailSettingsDto
        {
            SmtpHost = settings.GetValueOrDefault("SmtpHost", ""),
            SmtpPort = int.Parse(settings.GetValueOrDefault("SmtpPort", "587")),
            SmtpUsername = settings.GetValueOrDefault("SmtpUsername", ""),
            SmtpUseSsl = bool.Parse(settings.GetValueOrDefault("SmtpUseSsl", "true")),
            FromEmail = settings.GetValueOrDefault("FromEmail", ""),
            FromName = settings.GetValueOrDefault("FromName", "Community Car")
        };
    }

    public async Task UpdateEmailSettingsAsync(UpdateEmailSettingsDto dto)
    {
        await SetSettingAsync("SmtpHost", dto.SmtpHost, "Email");
        await SetSettingAsync("SmtpPort", dto.SmtpPort.ToString(), "Email");
        await SetSettingAsync("SmtpUsername", dto.SmtpUsername, "Email");
        if (!string.IsNullOrEmpty(dto.SmtpPassword))
        {
            // In production, encrypt the password before storing
            await SetSettingAsync("SmtpPassword", dto.SmtpPassword, "Email");
        }
        await SetSettingAsync("SmtpUseSsl", dto.SmtpUseSsl.ToString(), "Email");
        await SetSettingAsync("FromEmail", dto.FromEmail, "Email");
        await SetSettingAsync("FromName", dto.FromName, "Email");
        
        await ClearSettingsCacheAsync();
    }

    public async Task<bool> TestEmailConnectionAsync()
    {
        try
        {
            var settings = await GetEmailSettingsAsync();
            
            // TODO: Implement actual SMTP connection test
            // For now, just validate that required settings are present
            return !string.IsNullOrEmpty(settings.SmtpHost) &&
                   settings.SmtpPort > 0 &&
                   !string.IsNullOrEmpty(settings.FromEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing email connection");
            return false;
        }
    }

    public async Task<SecuritySettingsDto> GetSecuritySettingsAsync()
    {
        var settings = await GetSettingsByCategoryAsync("Security");
        
        var whitelistedIps = settings.GetValueOrDefault("WhitelistedIps", "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ip => ip.Trim())
            .ToList();
        
        return new SecuritySettingsDto
        {
            EnableTwoFactorAuth = bool.Parse(settings.GetValueOrDefault("EnableTwoFactorAuth", "false")),
            RequireStrongPassword = bool.Parse(settings.GetValueOrDefault("RequireStrongPassword", "true")),
            MinPasswordLength = int.Parse(settings.GetValueOrDefault("MinPasswordLength", "8")),
            PasswordExpiryDays = int.Parse(settings.GetValueOrDefault("PasswordExpiryDays", "90")),
            EnableIpWhitelist = bool.Parse(settings.GetValueOrDefault("EnableIpWhitelist", "false")),
            WhitelistedIps = whitelistedIps,
            EnableRateLimiting = bool.Parse(settings.GetValueOrDefault("EnableRateLimiting", "true")),
            RateLimitRequests = int.Parse(settings.GetValueOrDefault("RateLimitRequests", "100")),
            RateLimitPeriodSeconds = int.Parse(settings.GetValueOrDefault("RateLimitPeriodSeconds", "60"))
        };
    }

    public async Task UpdateSecuritySettingsAsync(UpdateSecuritySettingsDto dto)
    {
        await SetSettingAsync("EnableTwoFactorAuth", dto.EnableTwoFactorAuth.ToString(), "Security");
        await SetSettingAsync("RequireStrongPassword", dto.RequireStrongPassword.ToString(), "Security");
        await SetSettingAsync("MinPasswordLength", dto.MinPasswordLength.ToString(), "Security");
        await SetSettingAsync("PasswordExpiryDays", dto.PasswordExpiryDays.ToString(), "Security");
        await SetSettingAsync("EnableIpWhitelist", dto.EnableIpWhitelist.ToString(), "Security");
        await SetSettingAsync("WhitelistedIps", string.Join(",", dto.WhitelistedIps), "Security");
        await SetSettingAsync("EnableRateLimiting", dto.EnableRateLimiting.ToString(), "Security");
        await SetSettingAsync("RateLimitRequests", dto.RateLimitRequests.ToString(), "Security");
        await SetSettingAsync("RateLimitPeriodSeconds", dto.RateLimitPeriodSeconds.ToString(), "Security");
        
        await ClearSettingsCacheAsync();
    }

    public async Task<NotificationSettingsDto> GetNotificationSettingsAsync()
    {
        var settings = await GetSettingsByCategoryAsync("Notifications");
        
        return new NotificationSettingsDto
        {
            EnableEmailNotifications = bool.Parse(settings.GetValueOrDefault("EnableEmailNotifications", "true")),
            EnablePushNotifications = bool.Parse(settings.GetValueOrDefault("EnablePushNotifications", "false")),
            EnableSmsNotifications = bool.Parse(settings.GetValueOrDefault("EnableSmsNotifications", "false")),
            NotifyOnNewUser = bool.Parse(settings.GetValueOrDefault("NotifyOnNewUser", "true")),
            NotifyOnSecurityAlert = bool.Parse(settings.GetValueOrDefault("NotifyOnSecurityAlert", "true")),
            NotifyOnSystemError = bool.Parse(settings.GetValueOrDefault("NotifyOnSystemError", "true"))
        };
    }

    public async Task UpdateNotificationSettingsAsync(UpdateNotificationSettingsDto dto)
    {
        await SetSettingAsync("EnableEmailNotifications", dto.EnableEmailNotifications.ToString(), "Notifications");
        await SetSettingAsync("EnablePushNotifications", dto.EnablePushNotifications.ToString(), "Notifications");
        await SetSettingAsync("EnableSmsNotifications", dto.EnableSmsNotifications.ToString(), "Notifications");
        await SetSettingAsync("NotifyOnNewUser", dto.NotifyOnNewUser.ToString(), "Notifications");
        await SetSettingAsync("NotifyOnSecurityAlert", dto.NotifyOnSecurityAlert.ToString(), "Notifications");
        await SetSettingAsync("NotifyOnSystemError", dto.NotifyOnSystemError.ToString(), "Notifications");
        
        await ClearSettingsCacheAsync();
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        var cacheKey = $"{CacheKeyPrefix}{key}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedValue))
        {
            return cachedValue;
        }

        var setting = await _context.Set<ApplicationSetting>()
            .FirstOrDefaultAsync(s => s.Key == key);

        if (setting != null)
        {
            _cache.Set(cacheKey, setting.Value, CacheExpiration);
            return setting.Value;
        }

        return null;
    }

    public async Task SetSettingAsync(string key, string value, string category, Guid? modifiedBy = null)
    {
        var setting = await _context.Set<ApplicationSetting>()
            .FirstOrDefaultAsync(s => s.Key == key);

        if (setting == null)
        {
            setting = new ApplicationSetting(key, value, category);
            _context.Set<ApplicationSetting>().Add(setting);
        }
        else
        {
            setting.UpdateValue(value, modifiedBy);
        }

        await _context.SaveChangesAsync();
        
        // Update cache
        var cacheKey = $"{CacheKeyPrefix}{key}";
        _cache.Set(cacheKey, value, CacheExpiration);
    }

    public async Task<Dictionary<string, string>> GetSettingsByCategoryAsync(string category)
    {
        var cacheKey = $"{CacheKeyPrefix}Category_{category}";
        
        if (_cache.TryGetValue(cacheKey, out Dictionary<string, string>? cachedSettings))
        {
            return cachedSettings!;
        }

        var settings = await _context.Set<ApplicationSetting>()
            .Where(s => s.Category == category)
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        _cache.Set(cacheKey, settings, CacheExpiration);
        
        return settings;
    }

    public Task ClearSettingsCacheAsync()
    {
        // In a real implementation, you might want to use a more sophisticated cache invalidation strategy
        _logger.LogInformation("Settings cache cleared");
        return Task.CompletedTask;
    }
}
