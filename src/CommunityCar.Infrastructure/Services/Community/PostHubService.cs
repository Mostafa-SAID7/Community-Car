using CommunityCar.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

public interface IPostHubService
{
    Task NotifyFriendsNewPostAsync(List<Guid> friendIds, object postData);
    Task NotifyPostCreatedAsync(Guid userId, object postData);
    Task NotifyPostUpdatedAsync(Guid userId, object postData);
    Task NotifyPostDeletedAsync(Guid userId, Guid postId);
    Task NotifyPostLikedAsync(Guid authorId, Guid likerId, string likerName, string likerProfilePicture, Guid postId, string postTitle);
    Task NotifyPostCommentedAsync(Guid authorId, Guid commenterId, string commenterName, string commenterProfilePicture, Guid postId, string postTitle, string commentContent);
    Task NotifyPostSharedAsync(Guid authorId, Guid sharerId, string sharerName, string sharerProfilePicture, Guid postId, string postTitle);
    Task UpdatePostEngagementAsync(Guid postId, int likeCount, int commentCount, int shareCount, int viewCount);
    Task BroadcastNewCommentAsync(Guid postId, object commentData);
    Task BroadcastCommentUpdatedAsync(Guid postId, Guid commentId, string newContent);
    Task BroadcastCommentDeletedAsync(Guid postId, Guid commentId);
    Task NotifyPostStatusChangedAsync(Guid postId, string newStatus, Guid authorId);
    Task NotifyPostPinnedAsync(Guid postId, bool isPinned, Guid authorId);
    Task NotifyCommentReplyAsync(Guid originalCommenterId, Guid replierId, string replierName, string replierProfilePicture, Guid postId, string postTitle, string replyContent);
    Task NotifyPostMilestoneAsync(Guid authorId, Guid postId, string milestoneType, int count);
}

public class PostHubService : IPostHubService
{
    private readonly IHubContext<PostHub> _hubContext;
    private readonly ILogger<PostHubService> _logger;

    public PostHubService(IHubContext<PostHub> hubContext, ILogger<PostHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyFriendsNewPostAsync(List<Guid> friendIds, object postData)
    {
        try
        {
            foreach (var friendId in friendIds)
            {
                await _hubContext.Clients.Group($"user_{friendId}")
                    .SendAsync("FriendPublishedPost", new
                    {
                        Type = "NewPost",
                        Post = postData,
                        Timestamp = DateTimeOffset.UtcNow
                    });
            }

            _logger.LogInformation("New post notification sent to {Count} friends", friendIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying friends about new post");
        }
    }

    public async Task NotifyPostCreatedAsync(Guid userId, object postData)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{userId}")
                .SendAsync("PostCreated", new
                {
                    Success = true,
                    Message = "Post created successfully!",
                    Post = postData,
                    Timestamp = DateTimeOffset.UtcNow
                });

            _logger.LogInformation("Post created notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user about post creation");
        }
    }

    public async Task NotifyPostUpdatedAsync(Guid userId, object postData)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{userId}")
                .SendAsync("PostUpdated", new
                {
                    Success = true,
                    Message = "Post updated successfully!",
                    Post = postData,
                    Timestamp = DateTimeOffset.UtcNow
                });

            _logger.LogInformation("Post updated notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user about post update");
        }
    }

    public async Task NotifyPostDeletedAsync(Guid userId, Guid postId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{userId}")
                .SendAsync("PostDeleted", new
                {
                    Success = true,
                    Message = "Post deleted successfully!",
                    PostId = postId,
                    Timestamp = DateTimeOffset.UtcNow
                });

            _logger.LogInformation("Post deleted notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user about post deletion");
        }
    }

    public async Task NotifyPostLikedAsync(Guid authorId, Guid likerId, string likerName, 
        string likerProfilePicture, Guid postId, string postTitle)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{authorId}")
                .SendAsync("PostLiked", new
                {
                    LikerId = likerId,
                    LikerName = likerName,
                    LikerProfilePicture = likerProfilePicture,
                    PostId = postId,
                    PostTitle = postTitle,
                    Timestamp = DateTimeOffset.UtcNow
                });

            _logger.LogInformation("Post liked notification sent to author {AuthorId}", authorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying author about post like");
        }
    }

    public async Task NotifyPostCommentedAsync(Guid authorId, Guid commenterId, string commenterName,
        string commenterProfilePicture, Guid postId, string postTitle, string commentContent)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{authorId}")
                .SendAsync("PostCommented", new
                {
                    CommenterId = commenterId,
                    CommenterName = commenterName,
                    CommenterProfilePicture = commenterProfilePicture,
                    PostId = postId,
                    PostTitle = postTitle,
                    CommentContent = commentContent,
                    Timestamp = DateTimeOffset.UtcNow
                });

            _logger.LogInformation("Post commented notification sent to author {AuthorId}", authorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying author about post comment");
        }
    }

    public async Task NotifyPostSharedAsync(Guid authorId, Guid sharerId, string sharerName,
        string sharerProfilePicture, Guid postId, string postTitle)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{authorId}")
                .SendAsync("PostShared", new
                {
                    SharerId = sharerId,
                    SharerName = sharerName,
                    SharerProfilePicture = sharerProfilePicture,
                    PostId = postId,
                    PostTitle = postTitle,
                    Timestamp = DateTimeOffset.UtcNow
                });

            _logger.LogInformation("Post shared notification sent to author {AuthorId}", authorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying author about post share");
        }
    }

    public async Task UpdatePostEngagementAsync(Guid postId, int likeCount, int commentCount, 
        int shareCount, int viewCount)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("PostEngagementUpdated", new
            {
                PostId = postId,
                LikeCount = likeCount,
                CommentCount = commentCount,
                ShareCount = shareCount,
                ViewCount = viewCount,
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Post engagement updated for post {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post engagement");
        }
    }

    public async Task BroadcastNewCommentAsync(Guid postId, object commentData)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("NewCommentAdded", new
            {
                PostId = postId,
                Comment = commentData,
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("New comment broadcasted for post {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting new comment");
        }
    }

    public async Task BroadcastCommentUpdatedAsync(Guid postId, Guid commentId, string newContent)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("CommentUpdated", new
            {
                PostId = postId,
                CommentId = commentId,
                NewContent = newContent,
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Comment updated broadcasted for post {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting comment update");
        }
    }

    public async Task BroadcastCommentDeletedAsync(Guid postId, Guid commentId)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("CommentDeleted", new
            {
                PostId = postId,
                CommentId = commentId,
                Timestamp = DateTimeOffset.UtcNow
            });

            _logger.LogInformation("Comment deleted broadcasted for post {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting comment deletion");
        }
    }

    public async Task NotifyPostStatusChangedAsync(Guid postId, string newStatus, Guid authorId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{authorId}")
                .SendAsync("PostStatusChanged", new
                {
                    PostId = postId,
                    NewStatus = newStatus,
                    Timestamp = DateTimeOffset.UtcNow
                });

            _logger.LogInformation("Post status changed notification sent for post {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying about post status change");
        }
    }

    public async Task NotifyPostPinnedAsync(Guid postId, bool isPinned, Guid authorId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{authorId}")
                .SendAsync("PostPinStatusChanged", new
                {
                    PostId = postId,
                    IsPinned = isPinned,
                    Message = isPinned ? "Post pinned successfully!" : "Post unpinned successfully!",
                    Timestamp = DateTimeOffset.UtcNow
                });

            _logger.LogInformation("Post pin status changed for post {PostId}", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying about post pin status change");
        }
    }

    public async Task NotifyCommentReplyAsync(Guid originalCommenterId, Guid replierId, string replierName,
        string replierProfilePicture, Guid postId, string postTitle, string replyContent)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{originalCommenterId}")
                .SendAsync("CommentReplyReceived", new
                {
                    ReplierId = replierId,
                    ReplierName = replierName,
                    ReplierProfilePicture = replierProfilePicture,
                    PostId = postId,
                    PostTitle = postTitle,
                    ReplyContent = replyContent,
                    Timestamp = DateTimeOffset.UtcNow
                });

            _logger.LogInformation("Comment reply notification sent to {OriginalCommenterId}", originalCommenterId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying about comment reply");
        }
    }

    public async Task NotifyPostMilestoneAsync(Guid authorId, Guid postId, string milestoneType, int count)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{authorId}")
                .SendAsync("PostMilestoneReached", new
                {
                    PostId = postId,
                    MilestoneType = milestoneType,
                    Count = count,
                    Message = $"Your post reached {count} {milestoneType}!",
                    Timestamp = DateTimeOffset.UtcNow
                });

            _logger.LogInformation("Post milestone notification sent to {AuthorId}", authorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying about post milestone");
        }
    }
}
