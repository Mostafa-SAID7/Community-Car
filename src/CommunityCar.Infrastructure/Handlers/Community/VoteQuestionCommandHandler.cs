using CommunityCar.Domain.Base;
using CommunityCar.Domain.Commands.Community;
using CommunityCar.Domain.Entities.Community.qa;
using CommunityCar.Domain.Interfaces;
using CommunityCar.Domain.Events.Community;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Handlers.Community;

/// <summary>
/// Handler for VoteQuestionCommand - implements idempotent voting logic
/// Scenarios:
/// - No Vote → Up: +1, Down: -1
/// - Up → Up: Remove (0)
/// - Down → Down: Remove (0)
/// - Up → Down: -2
/// - Down → Up: +2
/// </summary>
public class VoteQuestionCommandHandler : ICommandHandler<VoteQuestionCommand, VoteResult>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VoteQuestionCommandHandler> _logger;
    private readonly IDomainEventHandler<VoteCreatedEvent> _voteCreatedHandler;
    private readonly IDomainEventHandler<VoteChangedEvent> _voteChangedHandler;
    private readonly IDomainEventHandler<VoteRemovedEvent> _voteRemovedHandler;
    private readonly IDomainEventHandler<VoteResurrectedEvent> _voteResurrectedHandler;

    public VoteQuestionCommandHandler(
        ApplicationDbContext context,
        ILogger<VoteQuestionCommandHandler> logger,
        IDomainEventHandler<VoteCreatedEvent> voteCreatedHandler,
        IDomainEventHandler<VoteChangedEvent> voteChangedHandler,
        IDomainEventHandler<VoteRemovedEvent> voteRemovedHandler,
        IDomainEventHandler<VoteResurrectedEvent> voteResurrectedHandler)
    {
        _context = context;
        _logger = logger;
        _voteCreatedHandler = voteCreatedHandler;
        _voteChangedHandler = voteChangedHandler;
        _voteRemovedHandler = voteRemovedHandler;
        _voteResurrectedHandler = voteResurrectedHandler;
    }

    public async Task<Result<VoteResult>> HandleAsync(
        VoteQuestionCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate: User must be authenticated
            if (command.UserId == Guid.Empty)
            {
                return Result.Failure<VoteResult>("User must be authenticated");
            }

            // Validate: Question must exist
            var question = await _context.Set<Question>()
                .FirstOrDefaultAsync(q => q.Id == command.QuestionId, cancellationToken);

            if (question == null)
            {
                return Result.Failure<VoteResult>("Question not found");
            }

            // Check for existing vote (including soft-deleted ones)
            var existingVote = await _context.Set<QuestionVote>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    v => v.QuestionId == command.QuestionId && v.UserId == command.UserId,
                    cancellationToken);

            VoteResult result;

            Guid voteId = existingVote?.Id ?? Guid.Empty;

            if (existingVote == null)
            {
                // Scenario: No Vote → Create Vote
                (result, voteId) = await CreateVoteAsync(question, command, cancellationToken);
            }
            else if (existingVote.IsDeleted)
            {
                // Scenario: Soft-deleted vote exists → Resurrect or Change
                result = await ResurrectOrChangeVoteAsync(question, existingVote, command.IsUpvote, cancellationToken);
            }
            else if (existingVote.IsUpvote == command.IsUpvote)
            {
                // Scenario: Same Vote → Soft Delete (Toggle off)
                result = await SoftDeleteVoteAsync(question, existingVote, cancellationToken);
            }
            else
            {
                // Scenario: Different Vote → Change Vote
                result = await ChangeVoteAsync(question, existingVote, command.IsUpvote, cancellationToken);
            }

            // Save changes atomically
            await _context.SaveChangesAsync(cancellationToken);

            // Dispatch domain events for audit trail and analytics
            await DispatchDomainEventsAsync(result.Action, voteId, question, command, result, cancellationToken);

            _logger.LogInformation(
                "Question {QuestionId} vote by user {UserId}: Action={Action}, Delta={Delta}, NewScore={Score}",
                command.QuestionId,
                command.UserId,
                result.Action,
                result.ScoreDelta,
                result.TotalScore);

            return Result<VoteResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing vote for question {QuestionId} by user {UserId}",
                command.QuestionId,
                command.UserId);

            return Result.Failure<VoteResult>("Failed to process vote");
        }
    }

    /// <summary>
    /// Creates a new vote when user has no existing vote
    /// No Vote → Up: +1, Down: -1
    /// </summary>
    private async Task<(VoteResult, Guid)> CreateVoteAsync(
        Question question,
        VoteQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var vote = new QuestionVote(command.QuestionId, command.UserId, command.IsUpvote);
        await _context.Set<QuestionVote>().AddAsync(vote, cancellationToken);

        int delta = command.IsUpvote ? 1 : -1;
        question.UpdateVoteCount(delta);

        var result = new VoteResult
        {
            CurrentVote = command.IsUpvote,
            TotalScore = question.VoteCount,
            ScoreDelta = delta,
            Action = VoteAction.Added
        };

        return (result, vote.Id);
    }

    /// <summary>
    /// Soft deletes an existing vote when user clicks same vote again
    /// Up → Up: Remove (0), Down → Down: Remove (0)
    /// </summary>
    private Task<VoteResult> SoftDeleteVoteAsync(
        Question question,
        QuestionVote existingVote,
        CancellationToken cancellationToken)
    {
        existingVote.IsDeleted = true;
        existingVote.DeletedAt = DateTimeOffset.UtcNow;

        int delta = existingVote.IsUpvote ? -1 : 1;
        question.UpdateVoteCount(delta);

        _logger.LogInformation(
            "Soft deleted vote for question {QuestionId} by user {UserId}",
            question.Id,
            existingVote.UserId);

        return Task.FromResult(new VoteResult
        {
            CurrentVote = null,
            TotalScore = question.VoteCount,
            ScoreDelta = delta,
            Action = VoteAction.Removed
        });
    }

    /// <summary>
    /// Changes vote direction when user clicks opposite vote
    /// Up → Down: -2, Down → Up: +2
    /// </summary>
    private Task<VoteResult> ChangeVoteAsync(
        Question question,
        QuestionVote existingVote,
        bool newIsUpvote,
        CancellationToken cancellationToken)
    {
        existingVote.Toggle();
        existingVote.ModifiedAt = DateTimeOffset.UtcNow;

        int delta = newIsUpvote ? 2 : -2;
        question.UpdateVoteCount(delta);

        return Task.FromResult(new VoteResult
        {
            CurrentVote = newIsUpvote,
            TotalScore = question.VoteCount,
            ScoreDelta = delta,
            Action = VoteAction.Switched
        });
    }

    /// <summary>
    /// Resurrects a soft-deleted vote or changes its direction
    /// Handles cases where user previously voted, removed it, and now votes again
    /// </summary>
    private Task<VoteResult> ResurrectOrChangeVoteAsync(
        Question question,
        QuestionVote existingVote,
        bool newIsUpvote,
        CancellationToken cancellationToken)
    {
        // Resurrect the vote
        existingVote.IsDeleted = false;
        existingVote.DeletedAt = null;
        existingVote.ModifiedAt = DateTimeOffset.UtcNow;

        VoteAction action;
        int delta;

        if (existingVote.IsUpvote == newIsUpvote)
        {
            // Same vote as before → Just resurrect
            delta = newIsUpvote ? 1 : -1;
            action = VoteAction.Added;
        }
        else
        {
            // Different vote → Resurrect and change
            existingVote.Toggle();
            delta = newIsUpvote ? 1 : -1;
            action = VoteAction.Switched;
        }

        question.UpdateVoteCount(delta);

        _logger.LogInformation(
            "Resurrected vote for question {QuestionId} by user {UserId}, IsUpvote={IsUpvote}",
            question.Id,
            existingVote.UserId,
            newIsUpvote);

        return Task.FromResult(new VoteResult
        {
            CurrentVote = newIsUpvote,
            TotalScore = question.VoteCount,
            ScoreDelta = delta,
            Action = action
        });
    }

    /// <summary>
    /// Dispatches domain events for audit trail and analytics
    /// </summary>
    private async Task DispatchDomainEventsAsync(
        VoteAction action,
        Guid voteId,
        Question question,
        VoteQuestionCommand command,
        VoteResult result,
        CancellationToken cancellationToken)
    {
        try
        {
            switch (action)
            {
                case VoteAction.Added:
                    var createdEvent = new VoteCreatedEvent(
                        voteId,
                        question.Id,
                        "Question",
                        command.UserId,
                        command.IsUpvote,
                        result.ScoreDelta,
                        result.TotalScore);
                    await _voteCreatedHandler.HandleAsync(createdEvent, cancellationToken);
                    break;

                case VoteAction.Switched:
                    var changedEvent = new VoteChangedEvent(
                        voteId,
                        question.Id,
                        "Question",
                        command.UserId,
                        !command.IsUpvote, // old value
                        command.IsUpvote,  // new value
                        result.ScoreDelta,
                        result.TotalScore);
                    await _voteChangedHandler.HandleAsync(changedEvent, cancellationToken);
                    break;

                case VoteAction.Removed:
                    var removedEvent = new VoteRemovedEvent(
                        voteId,
                        question.Id,
                        "Question",
                        command.UserId,
                        command.IsUpvote,
                        result.ScoreDelta,
                        result.TotalScore);
                    await _voteRemovedHandler.HandleAsync(removedEvent, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching domain events for question vote {VoteId}", voteId);
            // Don't throw - audit logging failure shouldn't break the vote operation
        }
    }
}
