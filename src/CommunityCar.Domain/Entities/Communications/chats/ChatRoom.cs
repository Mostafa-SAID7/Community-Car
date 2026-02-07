using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Domain.Entities.Communications.chats;

public class ChatRoom : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
    public Guid? CreatedBy { get; set; }
    public virtual ApplicationUser? Creator { get; set; }
    
    public virtual ICollection<ChatRoomMember> Members { get; set; } = new List<ChatRoomMember>();
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
