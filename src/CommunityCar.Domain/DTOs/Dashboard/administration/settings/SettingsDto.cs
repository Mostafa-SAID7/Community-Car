namespace CommunityCar.Domain.DTOs.Dashboard.Administration.Settings;

public class ApplicationSettingsDto
{
    public string ApplicationName { get; set; } = string.Empty;
    public string ApplicationUrl { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public bool MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public bool AllowRegistration { get; set; }
    public bool RequireEmailConfirmation { get; set; }
    public int SessionTimeout { get; set; }
    public int MaxLoginAttempts { get; set; }
    public int LockoutDuration { get; set; }
}

public class EmailSettingsDto
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public bool SmtpUseSsl { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class SecuritySettingsDto
{
    public bool EnableTwoFactorAuth { get; set; }
    public bool RequireStrongPassword { get; set; }
    public int MinPasswordLength { get; set; }
    public int PasswordExpiryDays { get; set; }
    public bool EnableIpWhitelist { get; set; }
    public List<string> WhitelistedIps { get; set; } = new();
    public bool EnableRateLimiting { get; set; }
    public int RateLimitRequests { get; set; }
    public int RateLimitPeriodSeconds { get; set; }
}

public class NotificationSettingsDto
{
    public bool EnableEmailNotifications { get; set; }
    public bool EnablePushNotifications { get; set; }
    public bool EnableSmsNotifications { get; set; }
    public bool NotifyOnNewUser { get; set; }
    public bool NotifyOnSecurityAlert { get; set; }
    public bool NotifyOnSystemError { get; set; }
}

public class UpdateApplicationSettingsDto
{
    public string ApplicationName { get; set; } = string.Empty;
    public string ApplicationUrl { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public bool MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public bool AllowRegistration { get; set; }
    public bool RequireEmailConfirmation { get; set; }
    public int SessionTimeout { get; set; }
    public int MaxLoginAttempts { get; set; }
    public int LockoutDuration { get; set; }
}

public class UpdateEmailSettingsDto
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string? SmtpPassword { get; set; }
    public bool SmtpUseSsl { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class UpdateSecuritySettingsDto
{
    public bool EnableTwoFactorAuth { get; set; }
    public bool RequireStrongPassword { get; set; }
    public int MinPasswordLength { get; set; }
    public int PasswordExpiryDays { get; set; }
    public bool EnableIpWhitelist { get; set; }
    public List<string> WhitelistedIps { get; set; } = new();
    public bool EnableRateLimiting { get; set; }
    public int RateLimitRequests { get; set; }
    public int RateLimitPeriodSeconds { get; set; }
}

public class UpdateNotificationSettingsDto
{
    public bool EnableEmailNotifications { get; set; }
    public bool EnablePushNotifications { get; set; }
    public bool EnableSmsNotifications { get; set; }
    public bool NotifyOnNewUser { get; set; }
    public bool NotifyOnSecurityAlert { get; set; }
    public bool NotifyOnSystemError { get; set; }
}
