using CommunityCar.Domain.Entities.Communications.notifications;
using CommunityCar.Domain.Entities.Community.qa;

namespace CommunityCar.Domain.Interfaces.Communications;

public interface INotificationService
{
    Task CreateNotificationAsync(Guid userId, string title, string message, string? link = null);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, int count = 10);
    Task<IEnumerable<Notification>> GetUserNotificationsPaginatedAsync(Guid userId, int page = 1, int pageSize = 20);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task DeleteNotificationAsync(Guid notificationId);
    Task DeleteAllNotificationsAsync(Guid userId);
    Task DeleteReadNotificationsAsync(Guid userId);
    
    // Domain specific notifications
    Task NotifyFriendsOfNewQuestionAsync(Guid authorId, Question question);
    Task NotifyFriendsOfQuestionUpdateAsync(Guid authorId, Question question);
    Task NotifyFriendsOfQuestionDeleteAsync(Guid authorId, string questionTitle);
    
    Task NotifyAuthorOfQuestionVoteAsync(Guid authorId, Question question, bool isUpvote);
    Task NotifyAuthorOfAnswerVoteAsync(Guid authorId, Answer answer, bool isUpvote);
    
    Task NotifyAuthorOfQuestionReactionAsync(Guid authorId, Question question, string reactionType);
    Task NotifyAuthorOfAnswerReactionAsync(Guid authorId, Answer answer, string reactionType);
    
    Task NotifyAuthorOfQuestionBookmarkAsync(Guid authorId, Question question);
    Task NotifyAuthorOfQuestionShareAsync(Guid authorId, Question question);
    
    Task NotifyAuthorOfNewAnswerAsync(Guid questionAuthorId, Answer answer);
    Task NotifyAuthorOfNewCommentAsync(Guid answerAuthorId, AnswerComment comment);
    
    // Chat notifications
    Task NotifyUserOfNewMessageAsync(Guid userId, Guid senderId, string senderName, string messagePreview);
    
    // Friend notifications
    Task NotifyUserOfFriendRequestAsync(Guid userId, Guid requesterId, string requesterName);
    Task NotifyUserOfFriendRequestAcceptedAsync(Guid userId, Guid accepterId, string accepterName);
}
