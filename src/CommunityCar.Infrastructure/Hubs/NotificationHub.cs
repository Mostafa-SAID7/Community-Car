using CommunityCar.Infrastructure.Hubs.Base;
using CommunityCar.Infrastructure.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Hubs;


[Authorize]
public class NotificationHub : BaseHub<NotificationHub>
{
    public NotificationHub(ILogger<NotificationHub> logger, IConnectionManager connectionManager) : base(logger, connectionManager)
    {
    }

    
    public override async Task JoinGroup(string groupName)
    {
        await base.JoinGroup(groupName);
    }

    
    public override async Task LeaveGroup(string groupName)
    {
        await base.LeaveGroup(groupName);
    }

    #region Utility Methods

    /// <summary>
    /// Get count of users in a specific notification group
    /// </summary>
    public int GetGroupMemberCount(string groupName)
    {
        return ConnectionManager.GetGroupConnectionCount(groupName);
    }

    #endregion
}
