using System;
using System.Threading.Tasks;

namespace CommunityCar.Domain.Interfaces.Services
{
    public interface IAssistantService
    {
        Task<string> GetChatResponseAsync(string userId, string message);
    }
}
