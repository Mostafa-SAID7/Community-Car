namespace CommunityCar.Domain.DTOs.Communications;

public class ChatRoomDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsGroup { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessage { get; set; }
    public int UnreadCount { get; set; }
    public IEnumerable<Guid> ParticipantIds { get; set; } = new List<Guid>();
    public string? AvatarUrl { get; set; }
}
