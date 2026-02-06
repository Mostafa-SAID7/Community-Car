namespace CommunityCar.Domain.DTOs.Community;

public class QuestionBookmarkDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public string? QuestionSlug { get; set; }
    public Guid UserId { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
