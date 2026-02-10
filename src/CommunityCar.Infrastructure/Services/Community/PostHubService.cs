using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Infrastructure.Hubs;
using CommunityCar.Infrastructure.Services.Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

/// <summary>
/// Service for sending real-time post-related notifications via PostHub
/// Follows Clean Architecture with proper separation of concerns
/// </summary>
public class PostHubService : IPostHubService
{
    private readonly IHubContext<PostHub> _hubContext;
    private readonly ILogger<PostHubService> _logger;
    private readonly IConnectionManager _connectionManager;

    public PostHubService(IHubContext<PostHub> hubContext, ILogger<PostHubService> logger, IConnectionManager connectionManager)
    {
        _hubContext = hubContext;
        _logger = logger;
        _connectionManager = connectionManager;
    }

    public async Task NotifyFriendsNewPostAsync(List<Guid> friendIds, object postData)
    {
        try
        {
            var groups = friendIds.Select(id => $"user_{id}").ToList();
            if (groups.Any())
            {
                await _hubContext.Clients.Groups(groups)
                    .SendCoreAsync("FriendPublishedPost", new object[] { new
                    {
                        Type = "NewPost",
                        Post = postData,
                        Timestamp = DateTimeOffset.UtcNow
                    } });

                _logger.LogInformation("New post notification sent to {Count} friends via groups", friendIds.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying friends about new post");
            throw;
        }
    }

    public async Task NotifyPostCreatedAsync(Guid userId, object postData)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{userId}")
                .SendCoreAsync("PostCreated", new object[] { new
                {
                    Success = true,
                    Message = "Post created successfully!",
                    Post = postData,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Post created notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user about post creation");
            throw;
        }
    }

    public async Task NotifyPostUpdatedAsync(Guid userId, object postData)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{userId}")
                .SendCoreAsync("PostUpdated", new object[] { new
                {
                    Success = true,
                    Message = "Post updated successfully!",
                    Post = postData,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Post updated notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user about post update");
            throw;
        }
    }

    public async Task NotifyPostDeletedAsync(Guid userId, Guid postId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{userId}")
                .SendCoreAsync("PostDeleted", new object[] { new
                {
                    Success = true,
                    Message = "Post deleted successfully!",
                    PostId = postId,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Post deleted notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user about post deletion");
            throw;
        }
    }

    public async Task NotifyPostLikedAsync(Guid authorId, Guid likerId, string likerName, 
        string likerProfilePicture, Guid postId, string postTitle)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{authorId}")
                .SendCoreAsync("PostLiked", new object[] { new
                {
                    LikerId = likerId,
                    LikerName = likerName,
                    LikerProfilePicture = likerProfilePicture,
                    PostId = postId,
                    PostTitle = postTitle,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Post liked notification sent to author {AuthorId}", authorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying author about post like");
            throw;
        }
    }

    public async Task NotifyPostCommentedAsync(Guid authorId, Guid commenterId, string commenterName,
        string commenterProfilePicture, Guid postId, string postTitle, string commentContent)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{authorId}")
                .SendCoreAsync("PostCommented", new object[] { new
                {
                    CommenterId = commenterId,
                    CommenterName = commenterName,
                    CommenterProfilePicture = commenterProfilePicture,
                    PostId = postId,
                    PostTitle = postTitle,
                    CommentContent = commentContent,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Post commented notification sent to author {AuthorId}", authorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying author about post comment");
            throw;
        }
    }

    public async Task NotifyPostSharedAsync(Guid authorId, Guid sharerId, string sharerName,
        string sharerProfilePicture, Guid postId, string postTitle)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{authorId}")
                .SendCoreAsync("PostShared", new object[] { new
                {
                    SharerId = sharerId,
                    SharerName = sharerName,
                    SharerProfilePicture = sharerProfilePicture,
                    PostId = postId,
                    PostTitle = postTitle,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Post shared notification sent to author {AuthorId}", authorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying author about post share");
            throw;
        }
    }

    public async Task UpdatePostEngagementAsync(Guid postId, int likeCount, int commentCount, 
        int shareCount, int viewCount)
    {
        try
        {
            await _hubContext.Clients.All.SendCoreAsync("PostEngagementUpdated", new object[] { new
            {
                PostId = postId,
                LikeCount = likeCount,
                CommentCount = commentCount,
                ShareCount = shareCount,
                ViewCount = viewCount,
                Timestamp = DateTimeOffset.UtcNow
            } });

            _logger.LogInformation("Post engagement updated for post {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post engagement");
            throw;
        }
    }

    public async Task BroadcastNewCommentAsync(Guid postId, object commentData)
    {
        try
        {
            await _hubContext.Clients.Group($"post_{postId}").SendCoreAsync("NewCommentAdded", new object[] { new
            {
                PostId = postId,
                Comment = commentData,
                Timestamp = DateTimeOffset.UtcNow
            } });

            _logger.LogInformation("New comment broadcasted for post {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting new comment");
            throw;
        }
    }

    public async Task BroadcastCommentUpdatedAsync(Guid postId, Guid commentId, string newContent)
    {
        try
        {
            await _hubContext.Clients.Group($"post_{postId}").SendCoreAsync("CommentUpdated", new object[] { new
            {
                PostId = postId,
                CommentId = commentId,
                NewContent = newContent,
                Timestamp = DateTimeOffset.UtcNow
            } });

            _logger.LogInformation("Comment updated broadcasted for post {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting comment update");
            throw;
        }
    }

    public async Task BroadcastCommentDeletedAsync(Guid postId, Guid commentId)
    {
        try
        {
            await _hubContext.Clients.Group($"post_{postId}").SendCoreAsync("CommentDeleted", new object[] { new
            {
                PostId = postId,
                CommentId = commentId,
                Timestamp = DateTimeOffset.UtcNow
            } });

            _logger.LogInformation("Comment deleted broadcasted for post {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting comment deletion");
            throw;
        }
    }

    public async Task NotifyPostStatusChangedAsync(Guid postId, string newStatus, Guid authorId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{authorId}")
                .SendCoreAsync("PostStatusChanged", new object[] { new
                {
                    PostId = postId,
                    NewStatus = newStatus,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Post status changed notification sent for post {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying about post status change");
            throw;
        }
    }

    public async Task NotifyPostPinnedAsync(Guid postId, bool isPinned, Guid authorId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{authorId}")
                .SendCoreAsync("PostPinStatusChanged", new object[] { new
                {
                    PostId = postId,
                    IsPinned = isPinned,
                    Message = isPinned ? "Post pinned successfully!" : "Post unpinned successfully!",
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Post pin status changed for post {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying about post pin status change");
            throw;
        }
    }

    public async Task NotifyCommentReplyAsync(Guid originalCommenterId, Guid replierId, string replierName,
        string replierProfilePicture, Guid postId, string postTitle, string replyContent)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{originalCommenterId}")
                .SendCoreAsync("CommentReplyReceived", new object[] { new
                {
                    ReplierId = replierId,
                    ReplierName = replierName,
                    ReplierProfilePicture = replierProfilePicture,
                    PostId = postId,
                    PostTitle = postTitle,
                    ReplyContent = replyContent,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Comment reply notification sent to {OriginalCommenterId}", originalCommenterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying about comment reply");
            throw;
        }
    }

    public async Task NotifyPostMilestoneAsync(Guid authorId, Guid postId, string milestoneType, int count)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{authorId}")
                .SendCoreAsync("PostMilestoneReached", new object[] { new
                {
                    PostId = postId,
                    MilestoneType = milestoneType,
                    Count = count,
                    Message = $"Your post reached {count} {milestoneType}!",
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Post milestone notification sent to {AuthorId}", authorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying about post milestone");
            throw;
        }
    }
}
