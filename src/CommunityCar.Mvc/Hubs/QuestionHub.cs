using Microsoft.AspNetCore.SignalR;

namespace CommunityCar.Web.Hubs;

public class QuestionHub : Hub
{
    public async Task SendQuestion(object question)
    {
        await Clients.All.SendAsync("ReceiveQuestion", question);
    }
}
