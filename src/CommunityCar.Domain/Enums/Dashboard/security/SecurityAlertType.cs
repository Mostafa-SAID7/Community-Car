namespace CommunityCar.Domain.Enums.Dashboard.security;

public enum SecurityAlertType
{
    UnauthorizedAccess = 0,
    SuspiciousActivity = 1,
    DataBreach = 2,
    FailedLogin = 3,
    PasswordChange = 4,
    PermissionChange = 5,
    AccountLockout = 6,
    MaliciousRequest = 7,
    Other = 99
}
