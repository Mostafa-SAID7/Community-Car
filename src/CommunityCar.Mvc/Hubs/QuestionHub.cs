using Microsoft.AspNetCore.SignalR;

namespace CommunityCar.Mvc.Hubs;

public class QuestionHub : Hub
{
    public async Task SendQuestion(object question)
    {
        await Clients.All.SendAsync("ReceiveQuestion", question);
    }

    public async Task SendComment(object comment)
    {
        await Clients.All.SendAsync("ReceiveComment", comment);
    }

    public async Task UpdateComment(object comment)
    {
        await Clients.All.SendAsync("CommentUpdated", comment);
    }

    public async Task DeleteComment(Guid commentId)
    {
        await Clients.All.SendAsync("CommentDeleted", new { commentId });
    }
    public async Task SendAnswer(object answer)
    {
        await Clients.All.SendAsync("ReceiveAnswer", answer);
    }

    public async Task QuestionScoreUpdated(Guid questionId, int newScore)
    {
        await Clients.All.SendAsync("QuestionScoreUpdated", new { questionId, newScore });
    }

    public async Task AnswerScoreUpdated(Guid answerId, int newScore)
    {
        await Clients.All.SendAsync("AnswerScoreUpdated", new { answerId, newScore });
    }

    public async Task QuestionMarkedResolved(Guid questionId, bool isResolved)
    {
        await Clients.All.SendAsync("QuestionMarkedResolved", new { questionId, isResolved });
    }
}
