using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Community;

/// <summary>
/// Service for sending real-time Q&A notifications via QuestionHub
/// Follows Clean Architecture with proper separation of concerns
/// </summary>
public class QuestionHubService : IQuestionHubService
{
    private readonly IHubContext<QuestionHub> _hubContext;
    private readonly ILogger<QuestionHubService> _logger;

    public QuestionHubService(IHubContext<QuestionHub> hubContext, ILogger<QuestionHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastNewQuestionAsync(QuestionDto question)
    {
        try
        {
            await _hubContext.Clients.All.SendCoreAsync("ReceiveQuestion", new object[] { new
            {
                Question = question,
                Timestamp = DateTimeOffset.UtcNow
            } });

            _logger.LogInformation("New question {QuestionId} broadcasted", question.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting new question {QuestionId}", question.Id);
            throw;
        }
    }

    public async Task BroadcastNewAnswerAsync(AnswerDto answer)
    {
        try
        {
            await _hubContext.Clients.Group($"question_{answer.QuestionId}")
                .SendCoreAsync("ReceiveAnswer", new object[] { new
                {
                    QuestionId = answer.QuestionId,
                    Answer = answer,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("New answer {AnswerId} broadcasted to question {QuestionId}", 
                answer.Id, answer.QuestionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting new answer {AnswerId}", answer.Id);
            throw;
        }
    }

    public async Task BroadcastQuestionScoreUpdateAsync(Guid questionId, int newScore)
    {
        try
        {
            await _hubContext.Clients.All.SendCoreAsync("QuestionScoreUpdated", new object[] { new
            {
                QuestionId = questionId,
                NewScore = newScore,
                Timestamp = DateTimeOffset.UtcNow
            } });

            _logger.LogInformation("Question {QuestionId} score updated to {Score}", questionId, newScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question score for {QuestionId}", questionId);
            throw;
        }
    }

    public async Task BroadcastAnswerScoreUpdateAsync(Guid answerId, int newScore)
    {
        try
        {
            await _hubContext.Clients.All.SendCoreAsync("AnswerScoreUpdated", new object[] { new
            {
                AnswerId = answerId,
                NewScore = newScore,
                Timestamp = DateTimeOffset.UtcNow
            } });

            _logger.LogInformation("Answer {AnswerId} score updated to {Score}", answerId, newScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating answer score for {AnswerId}", answerId);
            throw;
        }
    }

    public async Task BroadcastQuestionResolvedAsync(Guid questionId, bool isResolved)
    {
        try
        {
            await _hubContext.Clients.All.SendCoreAsync("QuestionMarkedResolved", new object[] { new
            {
                QuestionId = questionId,
                IsResolved = isResolved,
                Timestamp = DateTimeOffset.UtcNow
            } });

            _logger.LogInformation("Question {QuestionId} marked as {Status}", 
                questionId, isResolved ? "resolved" : "unresolved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question resolved status for {QuestionId}", questionId);
            throw;
        }
    }

    public async Task NotifyNewAnswerAsync(Guid questionAuthorId, Guid answerId, Guid answerAuthorId, 
        string answerAuthorName, string questionTitle)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{questionAuthorId}")
                .SendCoreAsync("NewAnswer", new object[] { new
                {
                    AnswerId = answerId,
                    AnswerAuthorId = answerAuthorId,
                    AnswerAuthorName = answerAuthorName,
                    QuestionTitle = questionTitle,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("New answer notification sent to question author {AuthorId}", questionAuthorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying question author {AuthorId}", questionAuthorId);
            throw;
        }
    }

    public async Task NotifyQuestionVotedAsync(Guid questionAuthorId, Guid voterId, string voterName,
        Guid questionId, string questionTitle, int voteType)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{questionAuthorId}")
                .SendCoreAsync("QuestionVoted", new object[] { new
                {
                    VoterId = voterId,
                    VoterName = voterName,
                    QuestionId = questionId,
                    QuestionTitle = questionTitle,
                    VoteType = voteType,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Question vote notification sent to author {AuthorId}", questionAuthorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying question vote to author {AuthorId}", questionAuthorId);
            throw;
        }
    }

    public async Task NotifyAnswerVotedAsync(Guid answerAuthorId, Guid voterId, string voterName,
        Guid answerId, Guid questionId, int voteType)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{answerAuthorId}")
                .SendCoreAsync("AnswerVoted", new object[] { new
                {
                    VoterId = voterId,
                    VoterName = voterName,
                    AnswerId = answerId,
                    QuestionId = questionId,
                    VoteType = voteType,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Answer vote notification sent to author {AuthorId}", answerAuthorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying answer vote to author {AuthorId}", answerAuthorId);
            throw;
        }
    }

    public async Task NotifyAnswerAcceptedAsync(Guid answerAuthorId, Guid questionId, Guid answerId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{answerAuthorId}")
                .SendCoreAsync("AnswerAccepted", new object[] { new
                {
                    QuestionId = questionId,
                    AnswerId = answerId,
                    Message = "Your answer was accepted!",
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Answer accepted notification sent to author {AuthorId}", answerAuthorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying answer accepted to author {AuthorId}", answerAuthorId);
            throw;
        }
    }

    public async Task NotifyNewCommentAsync(Guid contentOwnerId, Guid commenterId, string commenterName,
        Guid questionId, string questionTitle, string commentContent)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{contentOwnerId}")
                .SendCoreAsync("NewCommentReceived", new object[] { new
                {
                    CommenterId = commenterId,
                    CommenterName = commenterName,
                    QuestionId = questionId,
                    QuestionTitle = questionTitle,
                    CommentContent = commentContent,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("New comment notification sent to content owner {OwnerId}", contentOwnerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying new comment to owner {OwnerId}", contentOwnerId);
            throw;
        }
    }

    public async Task BroadcastCommentToThreadAsync(Guid questionId, object commentData)
    {
        try
        {
            await _hubContext.Clients.Group($"question_{questionId}")
                .SendCoreAsync("ReceiveComment", new object[] { new
                {
                    QuestionId = questionId,
                    Comment = commentData,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("New comment broadcasted to question {QuestionId}", questionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting comment to question {QuestionId}", questionId);
            throw;
        }
    }

    public async Task BroadcastCommentUpdatedAsync(Guid questionId, object commentData)
    {
        try
        {
            await _hubContext.Clients.Group($"question_{questionId}")
                .SendCoreAsync("CommentUpdated", new object[] { new
                {
                    QuestionId = questionId,
                    Comment = commentData,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Comment updated in question {QuestionId}", questionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting comment update for question {QuestionId}", questionId);
            throw;
        }
    }

    public async Task BroadcastCommentDeletedAsync(Guid questionId, Guid commentId)
    {
        try
        {
            await _hubContext.Clients.Group($"question_{questionId}")
                .SendCoreAsync("CommentDeleted", new object[] { new
                {
                    QuestionId = questionId,
                    CommentId = commentId,
                    Timestamp = DateTimeOffset.UtcNow
                } });

            _logger.LogInformation("Comment {CommentId} deleted from question {QuestionId}", 
                commentId, questionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting comment deletion for question {QuestionId}", questionId);
            throw;
        }
    }
}
