using CommunityCar.Domain.Base;
using CommunityCar.Domain.Entities.Identity.Users;
using CommunityCar.Domain.Entities.Community.Common;

namespace CommunityCar.Domain.Entities.Community.qa;

public class Question : AggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public Guid AuthorId { get; private set; }
    public virtual ApplicationUser Author { get; private set; } = null!;
    
    public Guid? CategoryId { get; private set; }
    public virtual Category? Category { get; private set; }
    
    public string? Tags { get; private set; } // Legacy field for backward compatibility
    public int ViewCount { get; private set; }
    public int VoteCount { get; private set; }
    public bool IsResolved { get; private set; }
    public Guid? AcceptedAnswerId { get; private set; }
    
    public int AnswerCount => Answers?.Count ?? 0;
    
    public virtual ICollection<Answer> Answers { get; private set; } = new List<Answer>();
    public virtual ICollection<QuestionVote> Votes { get; private set; } = new List<QuestionVote>();
    public virtual ICollection<QuestionBookmark> Bookmarks { get; private set; } = new List<QuestionBookmark>();
    public virtual ICollection<QuestionReaction> Reactions { get; private set; } = new List<QuestionReaction>();
    public virtual ICollection<QuestionShare> Shares { get; private set; } = new List<QuestionShare>();
    public virtual ICollection<QuestionTag> QuestionTags { get; private set; } = new List<QuestionTag>();

    private Question() { }

    public Question(string title, string content, Guid authorId, Guid? categoryId = null, string? tags = null)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Guard.Against.Empty(authorId, nameof(authorId));

        Title = title;
        Content = content;
        AuthorId = authorId;
        CategoryId = categoryId;
        Tags = tags;
        ViewCount = 0;
        VoteCount = 0;
        IsResolved = false;
    }

    public void Update(string title, string content, Guid? categoryId = null, string? tags = null)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));

        Title = title;
        Content = content;
        CategoryId = categoryId;
        Tags = tags;
    }

    public void SetCategory(Guid? categoryId) => CategoryId = categoryId;

    public void IncrementViewCount() => ViewCount++;

    public void MarkAsResolved(Guid answerId)
    {
        AcceptedAnswerId = answerId;
        IsResolved = true;
    }

    public void MarkAsUnresolved()
    {
        AcceptedAnswerId = null;
        IsResolved = false;
    }

    public void UpdateVoteCount(int delta) => VoteCount += delta;
}

