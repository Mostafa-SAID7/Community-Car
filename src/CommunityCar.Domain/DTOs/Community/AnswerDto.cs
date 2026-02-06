namespace CommunityCar.Domain.DTOs.Community;

public class AnswerDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorProfilePicture { get; set; }
    public bool AuthorIsExpert { get; set; }
    public string AuthorRankName { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public int ReactionCount { get; set; }
    public bool IsAccepted { get; set; }
    public int CurrentUserVote { get; set; } // 1 for up, -1 for down, 0 for none
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public IEnumerable<AnswerCommentDto> Comments { get; set; } = new List<AnswerCommentDto>();
}
