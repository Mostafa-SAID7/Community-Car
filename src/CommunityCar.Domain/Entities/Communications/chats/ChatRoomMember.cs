using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;

namespace CommunityCar.Domain.Entities.Communications.chats;

public class ChatRoomMember : BaseEntity
{
    public Guid ChatRoomId { get; set; }
    public virtual ChatRoom ChatRoom { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
    
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastReadAt { get; set; }
    public bool IsMuted { get; set; }
}
