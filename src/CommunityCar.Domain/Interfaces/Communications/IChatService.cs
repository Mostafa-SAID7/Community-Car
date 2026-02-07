using CommunityCar.Domain.DTOs.Communications;

namespace CommunityCar.Domain.Interfaces.Communications;

public interface IChatService
{
    Task<List<ChatConversationDto>> GetConversationsAsync(Guid userId);
    Task<List<ChatMessageDto>> GetMessagesAsync(Guid userId, Guid otherUserId, int page = 1, int pageSize = 50);
    Task<ChatMessageDto> SendMessageAsync(Guid senderId, SendMessageDto dto);
    Task<bool> MarkAsReadAsync(Guid messageId, Guid userId);
    Task<bool> MarkConversationAsReadAsync(Guid userId, Guid otherUserId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> DeleteMessageAsync(Guid messageId, Guid userId);
}
