using CommunityCar.Infrastructure.Hubs.Base;
using CommunityCar.Infrastructure.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for real-time Q&A notifications and updates
/// Handles: Questions, Answers, Comments, Votes, and Resolution status
/// Inherits from BaseHub for consistent connection management, authorization, and logging
/// </summary>
[Authorize]
public class QuestionHub : BaseHub<QuestionHub>
{
    public QuestionHub(ILogger<QuestionHub> logger, IConnectionManager connectionManager) : base(logger, connectionManager)
    {
    }

    #region Question Thread Management

    /// <summary>
    /// Join a specific question thread group to receive real-time updates
    /// </summary>
    public async Task JoinQuestionThread(Guid questionId)
    {
        var groupName = $"question_{questionId}";
        await JoinGroup(groupName);
        
        Logger.LogInformation("User {UserId} joined question thread {QuestionId}", 
            GetUserId(), questionId);
    }

    /// <summary>
    /// Leave a specific question thread group
    /// </summary>
    public async Task LeaveQuestionThread(Guid questionId)
    {
        var groupName = $"question_{questionId}";
        await LeaveGroup(groupName);
        
        Logger.LogInformation("User {UserId} left question thread {QuestionId}", 
            GetUserId(), questionId);
    }

    /// <summary>
    /// Get count of users watching a specific question
    /// </summary>
    public int GetQuestionWatcherCount(Guid questionId)
    {
        var groupName = $"question_{questionId}";
        return ConnectionManager.GetGroupConnectionCount(groupName);
    }

    #endregion

    #region Connection Lifecycle Overrides

    protected override async Task OnUserConnected(Guid userId, string connectionId)
    {
        Logger.LogInformation("QuestionHub: User {UserId} connected with connection {ConnectionId}", 
            userId, connectionId);
        await base.OnUserConnected(userId, connectionId);
    }

    protected override async Task OnUserDisconnected(Guid userId, string connectionId, bool isLastConnection)
    {
        Logger.LogInformation("QuestionHub: User {UserId} disconnected. Last connection: {IsLast}", 
            userId, isLastConnection);
        await base.OnUserDisconnected(userId, connectionId, isLastConnection);
    }

    protected override async Task OnUserOnline(Guid userId)
    {
        await Clients.Others.SendCoreAsync("UserOnline", new object[] { new
        {
            UserId = userId,
            Context = "QuestionHub",
            Timestamp = DateTimeOffset.UtcNow
        } });
        
        Logger.LogInformation("QuestionHub: User {UserId} is now online", userId);
    }

    protected override async Task OnUserOffline(Guid userId)
    {
        await Clients.Others.SendCoreAsync("UserOffline", new object[] { new
        {
            UserId = userId,
            Context = "QuestionHub",
            Timestamp = DateTimeOffset.UtcNow
        } });
        
        Logger.LogInformation("QuestionHub: User {UserId} is now offline", userId);
    }

    #endregion
}
