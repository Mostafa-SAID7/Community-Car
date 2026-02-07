using CommunityCar.Domain.DTOs.Communications;
using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Web.Areas.Communications.ViewModels;

public class ChatIndexViewModel
{
    public List<ChatConversationDto> Conversations { get; set; } = new();
    public int UnreadCount { get; set; }
}

public class ChatConversationViewModel
{
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string? OtherUserProfilePicture { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = new();
    public List<ChatConversationDto> Conversations { get; set; } = new();
    public bool IsOnline { get; set; }
}

public class SendMessageViewModel
{
    [Required(ErrorMessage = "Receiver is required")]
    public Guid ReceiverId { get; set; }

    [Required(ErrorMessage = "Message content is required")]
    [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
    public string Content { get; set; } = string.Empty;
}

public class ChatMessageViewModel
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderProfilePicture { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsMine { get; set; }
}
