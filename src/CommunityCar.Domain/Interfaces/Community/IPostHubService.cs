namespace CommunityCar.Domain.Interfaces.Community;

/// <summary>
/// Service interface for real-time post-related notifications via PostHub
/// Follows Clean Architecture principles with IHubContext injection
/// </summary>
public interface IPostHubService
{
    // Friend notifications
    Task NotifyFriendsNewPostAsync(List<Guid> friendIds, object postData);
    
    // Post lifecycle notifications
    Task NotifyPostCreatedAsync(Guid userId, object postData);
    Task NotifyPostUpdatedAsync(Guid userId, object postData);
    Task NotifyPostDeletedAsync(Guid userId, Guid postId);
    
    // Engagement notifications
    Task NotifyPostLikedAsync(Guid authorId, Guid likerId, string likerName, string likerProfilePicture, Guid postId, string postTitle);
    Task NotifyPostCommentedAsync(Guid authorId, Guid commenterId, string commenterName, string commenterProfilePicture, Guid postId, string postTitle, string commentContent);
    Task NotifyPostSharedAsync(Guid authorId, Guid sharerId, string sharerName, string sharerProfilePicture, Guid postId, string postTitle);
    
    // Real-time updates
    Task UpdatePostEngagementAsync(Guid postId, int likeCount, int commentCount, int shareCount, int viewCount);
    
    // Comment broadcasts
    Task BroadcastNewCommentAsync(Guid postId, object commentData);
    Task BroadcastCommentUpdatedAsync(Guid postId, Guid commentId, string newContent);
    Task BroadcastCommentDeletedAsync(Guid postId, Guid commentId);
    
    // Status changes
    Task NotifyPostStatusChangedAsync(Guid postId, string newStatus, Guid authorId);
    Task NotifyPostPinnedAsync(Guid postId, bool isPinned, Guid authorId);
    
    // Comment replies
    Task NotifyCommentReplyAsync(Guid originalCommenterId, Guid replierId, string replierName, string replierProfilePicture, Guid postId, string postTitle, string replyContent);
    
    // Milestones
    Task NotifyPostMilestoneAsync(Guid authorId, Guid postId, string milestoneType, int count);
}
