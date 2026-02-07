using CommunityCar.Domain.DTOs.Community;

namespace CommunityCar.Domain.Interfaces.Community;

public interface IQuestionHubService
{
    Task BroadcastNewQuestionAsync(QuestionDto question);
    Task BroadcastNewAnswerAsync(AnswerDto answer);
    Task BroadcastQuestionScoreUpdateAsync(Guid questionId, int newScore);
    Task BroadcastAnswerScoreUpdateAsync(Guid answerId, int newScore);
    Task BroadcastQuestionResolvedAsync(Guid questionId, bool isResolved);
}
