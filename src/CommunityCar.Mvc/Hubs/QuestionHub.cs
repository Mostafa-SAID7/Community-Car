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
}
