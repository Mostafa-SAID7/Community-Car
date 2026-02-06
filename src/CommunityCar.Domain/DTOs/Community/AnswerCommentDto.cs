namespace CommunityCar.Domain.DTOs.Community;

public class AnswerCommentDto
{
    public Guid Id { get; set; }
    public Guid AnswerId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorProfilePicture { get; set; }
    public bool AuthorIsExpert { get; set; }
    public string AuthorRankName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public int VoteCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public List<AnswerCommentDto> Replies { get; set; } = new();
}
