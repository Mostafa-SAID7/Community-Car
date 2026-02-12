using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Domain.Interfaces.Community;

/// <summary>
/// Service interface for sending real-time Q&A notifications via QuestionHub
/// </summary>
public interface IQuestionHubService
{
    // Broadcast events
    Task BroadcastNewQuestionAsync(QuestionDto question);
    Task BroadcastNewAnswerAsync(AnswerDto answer);
    Task BroadcastQuestionScoreUpdateAsync(Guid questionId, int newScore);
    Task BroadcastAnswerScoreUpdateAsync(Guid answerId, int newScore);
    Task BroadcastQuestionResolvedAsync(Guid questionId, bool isResolved);
    
    // User-specific notifications
    Task NotifyNewAnswerAsync(Guid questionAuthorId, Guid answerId, Guid answerAuthorId, 
        string answerAuthorName, string questionTitle);
    Task NotifyQuestionVotedAsync(Guid questionAuthorId, Guid voterId, string voterName,
        Guid questionId, string questionTitle, int voteType);
    Task NotifyAnswerVotedAsync(Guid answerAuthorId, Guid voterId, string voterName,
        Guid answerId, Guid questionId, int voteType);
    Task NotifyAnswerAcceptedAsync(Guid answerAuthorId, Guid questionId, Guid answerId);
    Task NotifyNewCommentAsync(Guid contentOwnerId, Guid commenterId, string commenterName,
        Guid questionId, string questionTitle, string commentContent);
    
    // Thread-specific broadcasts
    Task BroadcastCommentToThreadAsync(Guid questionId, object commentData);
    Task BroadcastCommentUpdatedAsync(Guid questionId, object commentData);
    Task BroadcastCommentDeletedAsync(Guid questionId, Guid commentId);
}
