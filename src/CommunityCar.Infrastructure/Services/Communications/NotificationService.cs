using CommunityCar.Domain.Entities.Communications.notifications;
using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Interfaces.Common;
using CommunityCar.Domain.Interfaces.Communications;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CommunityCar.Infrastructure.Services.Communications;

public class NotificationService : INotificationService
{
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IFriendshipService _friendshipService;
    private readonly IUnitOfWork _uow;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        IRepository<Notification> notificationRepository,
        IFriendshipService friendshipService,
        IUnitOfWork uow,
        IHubContext<NotificationHub> hubContext)
    {
        _notificationRepository = notificationRepository;
        _friendshipService = friendshipService;
        _uow = uow;
        _hubContext = hubContext;
    }

    public async Task CreateNotificationAsync(Guid userId, string title, string message, string? link = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Link = link,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification);
        await _uow.SaveChangesAsync();

        // Broadcast real-time
        await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", new
        {
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Link,
            notification.CreatedAt,
            notification.IsRead
        });
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, int count = 10)
    {
        return await _notificationRepository.GetQueryable()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            _notificationRepository.Update(notification);
            await _uow.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _notificationRepository.WhereAsync(n => n.UserId == userId && !n.IsRead);
        foreach (var n in unread)
        {
            n.IsRead = true;
            _notificationRepository.Update(n);
        }
        await _uow.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _notificationRepository.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task NotifyFriendsOfNewQuestionAsync(Guid authorId, Question question)
    {
        var friends = await _friendshipService.GetFriendsAsync(authorId);
        var authorName = question.Author != null ? $"{question.Author.FirstName} {question.Author.LastName}" : "A friend";
        
        var title = "New Question from Friend";
        var message = $"{authorName} posted a new question: {question.Title}";
        var link = $"/Questions/Details/{question.Slug ?? question.Id.ToString()}";

        foreach (var friendship in friends)
        {
            var friendId = friendship.UserId == authorId ? friendship.FriendId : friendship.UserId;
            await CreateNotificationAsync(friendId, title, message, link);
        }
    }

    public async Task NotifyFriendsOfQuestionUpdateAsync(Guid authorId, Question question)
    {
        var friends = await _friendshipService.GetFriendsAsync(authorId);
        var title = "Question Updated";
        var message = $"A question you might be interested in was updated: {question.Title}";
        var link = $"/Questions/Details/{question.Slug ?? question.Id.ToString()}";

        foreach (var friendship in friends)
        {
            var friendId = friendship.UserId == authorId ? friendship.FriendId : friendship.UserId;
            await CreateNotificationAsync(friendId, title, message, link);
        }
    }

    public async Task NotifyFriendsOfQuestionDeleteAsync(Guid authorId, string questionTitle)
    {
        var friends = await _friendshipService.GetFriendsAsync(authorId);
        var title = "Question Removed";
        var message = $"A question by your friend was removed: {questionTitle}";

        foreach (var friendship in friends)
        {
            var friendId = friendship.UserId == authorId ? friendship.FriendId : friendship.UserId;
            await CreateNotificationAsync(friendId, title, message, null);
        }
    }

    public async Task NotifyAuthorOfQuestionVoteAsync(Guid authorId, Question question, bool isUpvote)
    {
        var voteType = isUpvote ? "upvoted" : "downvoted";
        var title = "New Vote on Question";
        var message = $"Someone {voteType} your question: {question.Title}";
        var link = $"/Questions/Details/{question.Slug ?? question.Id.ToString()}";
        
        await CreateNotificationAsync(authorId, title, message, link);
    }

    public async Task NotifyAuthorOfAnswerVoteAsync(Guid authorId, Answer answer, bool isUpvote)
    {
        var voteType = isUpvote ? "upvoted" : "downvoted";
        var title = "New Vote on Answer";
        var message = $"Someone {voteType} your answer.";
        var link = $"/Questions/Details/{answer.QuestionId}";
        
        await CreateNotificationAsync(authorId, title, message, link);
    }

    public async Task NotifyAuthorOfQuestionReactionAsync(Guid authorId, Question question, string reactionType)
    {
        var title = "New Reaction";
        var message = $"Someone reacted with {reactionType} to your question: {question.Title}";
        var link = $"/Questions/Details/{question.Slug ?? question.Id.ToString()}";
        
        await CreateNotificationAsync(authorId, title, message, link);
    }

    public async Task NotifyAuthorOfAnswerReactionAsync(Guid authorId, Answer answer, string reactionType)
    {
        var title = "New Reaction";
        var message = $"Someone reacted with {reactionType} to your answer.";
        var link = $"/Questions/Details/{answer.QuestionId}";
        
        await CreateNotificationAsync(authorId, title, message, link);
    }

    public async Task NotifyAuthorOfQuestionBookmarkAsync(Guid authorId, Question question)
    {
        var title = "Question Bookmarked";
        var message = $"Someone bookmarked your question: {question.Title}";
        var link = $"/Questions/Details/{question.Slug ?? question.Id.ToString()}";
        
        await CreateNotificationAsync(authorId, title, message, link);
    }

    public async Task NotifyAuthorOfQuestionShareAsync(Guid authorId, Question question)
    {
        var title = "Question Shared";
        var message = $"Someone shared your question: {question.Title}";
        var link = $"/Questions/Details/{question.Slug ?? question.Id.ToString()}";
        
        await CreateNotificationAsync(authorId, title, message, link);
    }

    public async Task NotifyAuthorOfNewAnswerAsync(Guid questionAuthorId, Answer answer)
    {
        var title = "New Answer";
        var message = $"Your question received a new answer.";
        var link = $"/Questions/Details/{answer.QuestionId}";
        
        await CreateNotificationAsync(questionAuthorId, title, message, link);
    }

    public async Task NotifyAuthorOfNewCommentAsync(Guid answerAuthorId, AnswerComment comment)
    {
        var title = "New Comment";
        var message = $"Your answer received a new comment.";
        // Use answerId if Answer navigation is not loaded
        var link = comment.Answer != null 
            ? $"/Questions/Details/{comment.Answer.QuestionId}" 
            : $"/Questions/Details/{comment.AnswerId}";
        
        await CreateNotificationAsync(answerAuthorId, title, message, link);
    }
}
