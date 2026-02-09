using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CommunityCar.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for real-time post-related notifications and updates
/// Handles: Post creation, updates, likes, comments, shares, and friend notifications
/// </summary>
[Authorize]
public class PostHub : Hub
{
    private static readonly Dictionary<Guid, List<string>> _userConnections = new();
    private readonly ILogger<PostHub> _logger;

    public PostHub(ILogger<PostHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            if (!_userConnections.ContainsKey(userId.Value))
            {
                _userConnections[userId.Value] = new List<string>();
            }
            
            _userConnections[userId.Value].Add(Context.ConnectionId);
            _logger.LogInformation("User {UserId} connected to PostHub with connection {ConnectionId}", 
                userId.Value, Context.ConnectionId);
            
            // Join user's personal group for targeted notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue && _userConnections.ContainsKey(userId.Value))
        {
            _userConnections[userId.Value].Remove(Context.ConnectionId);
            
            if (_userConnections[userId.Value].Count == 0)
            {
                _userConnections.Remove(userId.Value);
            }
            
            _logger.LogInformation("User {UserId} disconnected from PostHub", userId.Value);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Notify friends when a new post is published
    /// </summary>
    public async Task NotifyFriendsNewPost(List<Guid> friendIds, object postData)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        var notification = new
        {
            Type = "NewPost",
            AuthorId = userId.Value,
            Post = postData,
            Timestamp = DateTimeOffset.UtcNow
        };

        foreach (var friendId in friendIds)
        {
            await Clients.Group($"user_{friendId}").SendAsync("FriendPublishedPost", notification);
        }

        _logger.LogInformation("New post notification sent to {Count} friends by user {UserId}", 
            friendIds.Count, userId.Value);
    }

    /// <summary>
    /// Notify specific user about post creation success
    /// </summary>
    public async Task NotifyPostCreated(Guid userId, object postData)
    {
        await Clients.Group($"user_{userId}").SendAsync("PostCreated", new
        {
            Success = true,
            Message = "Post created successfully!",
            Post = postData,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Post created notification sent to user {UserId}", userId);
    }

    /// <summary>
    /// Notify specific user about post update success
    /// </summary>
    public async Task NotifyPostUpdated(Guid userId, object postData)
    {
        await Clients.Group($"user_{userId}").SendAsync("PostUpdated", new
        {
            Success = true,
            Message = "Post updated successfully!",
            Post = postData,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Post updated notification sent to user {UserId}", userId);
    }

    /// <summary>
    /// Notify specific user about post deletion success
    /// </summary>
    public async Task NotifyPostDeleted(Guid userId, Guid postId)
    {
        await Clients.Group($"user_{userId}").SendAsync("PostDeleted", new
        {
            Success = true,
            Message = "Post deleted successfully!",
            PostId = postId,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Post deleted notification sent to user {UserId}", userId);
    }

    /// <summary>
    /// Notify post author when someone likes their post
    /// </summary>
    public async Task NotifyPostLiked(Guid authorId, Guid likerId, string likerName, string likerProfilePicture, Guid postId, string postTitle)
    {
        await Clients.Group($"user_{authorId}").SendAsync("PostLiked", new
        {
            LikerId = likerId,
            LikerName = likerName,
            LikerProfilePicture = likerProfilePicture,
            PostId = postId,
            PostTitle = postTitle,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Post liked notification sent to author {AuthorId} from {LikerId}", authorId, likerId);
    }

    /// <summary>
    /// Notify post author when someone comments on their post
    /// </summary>
    public async Task NotifyPostCommented(Guid authorId, Guid commenterId, string commenterName, 
        string commenterProfilePicture, Guid postId, string postTitle, string commentContent)
    {
        await Clients.Group($"user_{authorId}").SendAsync("PostCommented", new
        {
            CommenterId = commenterId,
            CommenterName = commenterName,
            CommenterProfilePicture = commenterProfilePicture,
            PostId = postId,
            PostTitle = postTitle,
            CommentContent = commentContent,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Post commented notification sent to author {AuthorId} from {CommenterId}", 
            authorId, commenterId);
    }

    /// <summary>
    /// Notify post author when someone shares their post
    /// </summary>
    public async Task NotifyPostShared(Guid authorId, Guid sharerId, string sharerName, 
        string sharerProfilePicture, Guid postId, string postTitle)
    {
        await Clients.Group($"user_{authorId}").SendAsync("PostShared", new
        {
            SharerId = sharerId,
            SharerName = sharerName,
            SharerProfilePicture = sharerProfilePicture,
            PostId = postId,
            PostTitle = postTitle,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Post shared notification sent to author {AuthorId} from {SharerId}", 
            authorId, sharerId);
    }

    /// <summary>
    /// Real-time update of post engagement stats (likes, comments, shares, views)
    /// </summary>
    public async Task UpdatePostEngagement(Guid postId, int likeCount, int commentCount, int shareCount, int viewCount)
    {
        await Clients.All.SendAsync("PostEngagementUpdated", new
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

    /// <summary>
    /// Notify when a new comment is added to a post (for all viewers)
    /// </summary>
    public async Task BroadcastNewComment(Guid postId, object commentData)
    {
        await Clients.All.SendAsync("NewCommentAdded", new
        {
            PostId = postId,
            Comment = commentData,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("New comment broadcasted for post {PostId}", postId);
    }

    /// <summary>
    /// Notify when a comment is updated
    /// </summary>
    public async Task BroadcastCommentUpdated(Guid postId, Guid commentId, string newContent)
    {
        await Clients.All.SendAsync("CommentUpdated", new
        {
            PostId = postId,
            CommentId = commentId,
            NewContent = newContent,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Comment updated broadcasted for post {PostId}, comment {CommentId}", 
            postId, commentId);
    }

    /// <summary>
    /// Notify when a comment is deleted
    /// </summary>
    public async Task BroadcastCommentDeleted(Guid postId, Guid commentId)
    {
        await Clients.All.SendAsync("CommentDeleted", new
        {
            PostId = postId,
            CommentId = commentId,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Comment deleted broadcasted for post {PostId}, comment {CommentId}", 
            postId, commentId);
    }

    /// <summary>
    /// Notify when a post status changes (published, archived, etc.)
    /// </summary>
    public async Task NotifyPostStatusChanged(Guid postId, string newStatus, Guid authorId)
    {
        await Clients.Group($"user_{authorId}").SendAsync("PostStatusChanged", new
        {
            PostId = postId,
            NewStatus = newStatus,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Post status changed to {Status} for post {PostId}", newStatus, postId);
    }

    /// <summary>
    /// Notify when a post is pinned/unpinned
    /// </summary>
    public async Task NotifyPostPinned(Guid postId, bool isPinned, Guid authorId)
    {
        await Clients.Group($"user_{authorId}").SendAsync("PostPinStatusChanged", new
        {
            PostId = postId,
            IsPinned = isPinned,
            Message = isPinned ? "Post pinned successfully!" : "Post unpinned successfully!",
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Post {PostId} pin status changed to {IsPinned}", postId, isPinned);
    }

    /// <summary>
    /// Notify when someone replies to a comment
    /// </summary>
    public async Task NotifyCommentReply(Guid originalCommenterId, Guid replierId, string replierName,
        string replierProfilePicture, Guid postId, string postTitle, string replyContent)
    {
        await Clients.Group($"user_{originalCommenterId}").SendAsync("CommentReplyReceived", new
        {
            ReplierId = replierId,
            ReplierName = replierName,
            ReplierProfilePicture = replierProfilePicture,
            PostId = postId,
            PostTitle = postTitle,
            ReplyContent = replyContent,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Comment reply notification sent to {OriginalCommenterId} from {ReplierId}", 
            originalCommenterId, replierId);
    }

    /// <summary>
    /// Notify when a post reaches a milestone (e.g., 100 likes, 50 comments)
    /// </summary>
    public async Task NotifyPostMilestone(Guid authorId, Guid postId, string milestoneType, int count)
    {
        await Clients.Group($"user_{authorId}").SendAsync("PostMilestoneReached", new
        {
            PostId = postId,
            MilestoneType = milestoneType,
            Count = count,
            Message = $"Your post reached {count} {milestoneType}!",
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("Post milestone notification sent to {AuthorId}: {Count} {MilestoneType}", 
            authorId, count, milestoneType);
    }

    /// <summary>
    /// Broadcast system-wide post-related announcements
    /// </summary>
    public async Task BroadcastAnnouncement(string message, string type)
    {
        await Clients.All.SendAsync("PostSystemAnnouncement", new
        {
            Message = message,
            Type = type,
            Timestamp = DateTimeOffset.UtcNow
        });

        _logger.LogInformation("System announcement broadcasted: {Message}", message);
    }

    /// <summary>
    /// Check if a user is currently online
    /// </summary>
    public static bool IsUserOnline(Guid userId)
    {
        return _userConnections.ContainsKey(userId) && _userConnections[userId].Count > 0;
    }

    /// <summary>
    /// Get all connection IDs for a specific user
    /// </summary>
    public static List<string> GetConnectionIds(Guid userId)
    {
        return _userConnections.TryGetValue(userId, out var connections) 
            ? new List<string>(connections) 
            : new List<string>();
    }

    /// <summary>
    /// Get total number of online users
    /// </summary>
    public static int GetOnlineUserCount()
    {
        return _userConnections.Count;
    }

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }
}
