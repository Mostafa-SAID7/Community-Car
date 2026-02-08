using CommunityCar.Domain.Base.Interfaces;

namespace CommunityCar.Domain.Commands.Community;

/// <summary>
/// Command to vote on a question (upvote/downvote with toggle and switch logic)
/// </summary>
public class VoteQuestionCommand : ICommand<VoteResult>
{
    public Guid QuestionId { get; }
    public Guid UserId { get; }
    public bool IsUpvote { get; }

    public VoteQuestionCommand(Guid questionId, Guid userId, bool isUpvote)
    {
        QuestionId = questionId;
        UserId = userId;
        IsUpvote = isUpvote;
    }
}

/// <summary>
/// Command to vote on an answer (upvote/downvote with toggle and switch logic)
/// </summary>
public class VoteAnswerCommand : ICommand<VoteResult>
{
    public Guid AnswerId { get; }
    public Guid UserId { get; }
    public bool IsUpvote { get; }

    public VoteAnswerCommand(Guid answerId, Guid userId, bool isUpvote)
    {
        AnswerId = answerId;
        UserId = userId;
        IsUpvote = isUpvote;
    }
}

/// <summary>
/// Result of a vote operation
/// </summary>
public class VoteResult
{
    /// <summary>
    /// Current vote state: true = upvoted, false = downvoted, null = no vote
    /// </summary>
    public bool? CurrentVote { get; set; }
    
    /// <summary>
    /// Updated vote score
    /// </summary>
    public int TotalScore { get; set; }
    
    /// <summary>
    /// Score change from this operation
    /// </summary>
    public int ScoreDelta { get; set; }
    
    /// <summary>
    /// Action that was performed
    /// </summary>
    public VoteAction Action { get; set; }
}

/// <summary>
/// Type of vote action performed
/// </summary>
public enum VoteAction
{
    Added,      // No vote → Vote added
    Removed,    // Same vote → Vote removed
    Switched    // Different vote → Vote switched
}
