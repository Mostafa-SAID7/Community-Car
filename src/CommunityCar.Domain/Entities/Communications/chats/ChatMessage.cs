using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Enums.Communications.chats;

namespace CommunityCar.Domain.Entities.Communications.chats;

public class ChatMessage : BaseEntity
{
    public Guid ChatRoomId { get; set; }
    public virtual ChatRoom ChatRoom { get; set; } = null!;
    
    public Guid SenderId { get; set; }
    public virtual ApplicationUser Sender { get; set; } = null!;
    
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public string? AttachmentUrl { get; set; }
    
    public bool IsEdited { get; set; }
    public DateTimeOffset? EditedAt { get; set; }
    
    public Guid? ReplyToMessageId { get; set; }
    public virtual ChatMessage? ReplyToMessage { get; set; }
}
