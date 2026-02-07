using CommunityCar.Domain.DTOs.Community;
using CommunityCar.Domain.Interfaces.Community;
using CommunityCar.Mvc.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CommunityCar.Mvc.Services;

public class QuestionHubService : IQuestionHubService
{
    private readonly IHubContext<QuestionHub> _hubContext;

    public QuestionHubService(IHubContext<QuestionHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastNewQuestionAsync(QuestionDto question)
    {
         await _hubContext.Clients.All.SendAsync("ReceiveQuestion", question);
    }

    public async Task BroadcastNewAnswerAsync(AnswerDto answer)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveAnswer", answer);
    }

    public async Task BroadcastQuestionScoreUpdateAsync(Guid questionId, int newScore)
    {
        await _hubContext.Clients.All.SendAsync("QuestionScoreUpdated", new { questionId, newScore });
    }

    public async Task BroadcastAnswerScoreUpdateAsync(Guid answerId, int newScore)
    {
        await _hubContext.Clients.All.SendAsync("AnswerScoreUpdated", new { answerId, newScore });
    }

    public async Task BroadcastQuestionResolvedAsync(Guid questionId, bool isResolved)
    {
        await _hubContext.Clients.All.SendAsync("QuestionMarkedResolved", new { questionId, isResolved });
    }
}
