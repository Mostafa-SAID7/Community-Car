namespace CommunityCar.Domain.Interfaces.Community;

/// <summary>
/// Interface for entities that can be voted on
/// </summary>
public interface IVotable
{
    Guid Id { get; }
    int VoteCount { get; }
    void UpdateVoteCount(int delta);
    string GetEntityType();
}
