namespace CommunityCar.Domain.DTOs.Community;

public class QuestionDto
{
    public Guid Id { get; set; }
    public string? Slug { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorProfilePicture { get; set; }
    public bool AuthorIsExpert { get; set; }
    public string AuthorRankName { get; set; } = string.Empty;
    
    public Guid? CategoryId { get; set; }
    public CategoryDto? Category { get; set; }
    
    public string? Tags { get; set; } // Legacy field
    public List<TagDto> TagList { get; set; } = new();
    
    public int ViewCount { get; set; }
    public int VoteCount { get; set; }
    public int AnswerCount { get; set; }
    public int BookmarkCount { get; set; }
    public int ReactionCount { get; set; }
    public int ShareCount { get; set; }
    public bool IsResolved { get; set; }
    public Guid? AcceptedAnswerId { get; set; }
    
    // User-specific state (populated for current user)
    public int CurrentUserVote { get; set; } // 1 for up, -1 for down, 0 for none
    public bool IsBookmarkedByUser { get; set; }
    public int? CurrentUserReactionType { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
