using CommunityCar.Infrastructure.Hubs.Base;
using CommunityCar.Infrastructure.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Hubs;


[Authorize]
public class FriendHub : BaseHub<FriendHub>
{
    public FriendHub(ILogger<FriendHub> logger, IConnectionManager connectionManager) : base(logger, connectionManager)
    {
    }

    #region Group Management

 
    public override async Task JoinGroup(string groupName)
    {
        await base.JoinGroup(groupName);
    }


    public override async Task LeaveGroup(string groupName)
    {
        await base.LeaveGroup(groupName);
    }

    #endregion
}
