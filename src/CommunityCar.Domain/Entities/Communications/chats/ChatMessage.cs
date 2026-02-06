using CommunityCar.Domain.Base;

namespace CommunityCar.Domain.Entities.Communications.chats;

public class ChatMessage : BaseEntity
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}
