namespace CommunityCar.Domain.DTOs.Communications;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderProfilePicture { get; set; }
    public Guid ReceiverId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string? ReceiverProfilePicture { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ChatConversationDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public string? LastMessage { get; set; }
    public DateTimeOffset? LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
    public bool IsOnline { get; set; }
}

public class SendMessageDto
{
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
}
