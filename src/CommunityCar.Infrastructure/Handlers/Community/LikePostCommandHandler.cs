using CommunityCar.Domain.Base;
using CommunityCar.Domain.Commands.Community;
using CommunityCar.Domain.Entities.Community.post;
using CommunityCar.Domain.Enums.Community.qa;
using CommunityCar.Domain.Events.Community;
using CommunityCar.Domain.Interfaces;
using CommunityCar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Handlers.Community;

/// <summary>
/// Handler for LikePostCommand - implements toggle like/unlike functionality with domain events
/// </summary>
public class LikePostCommandHandler : ICommandHandler<LikePostCommand, LikePostResult>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LikePostCommandHandler> _logger;
    private readonly IDomainEventHandler<LikeCreatedEvent> _likeCreatedHandler;
    private readonly IDomainEventHandler<LikeRemovedEvent> _likeRemovedHandler;

    public LikePostCommandHandler(
        ApplicationDbContext context,
        ILogger<LikePostCommandHandler> logger,
        IDomainEventHandler<LikeCreatedEvent> likeCreatedHandler,
        IDomainEventHandler<LikeRemovedEvent> likeRemovedHandler)
    {
        _context = context;
        _logger = logger;
        _likeCreatedHandler = likeCreatedHandler;
        _likeRemovedHandler = likeRemovedHandler;
    }

    public async Task<Result<LikePostResult>> HandleAsync(
        LikePostCommand command, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate: User must be authenticated
            if (command.UserId == Guid.Empty)
            {
                return Result.Failure<LikePostResult>("User must be authenticated");
            }

            // Validate: Post must exist
            var post = await _context.Set<Post>()
                .FirstOrDefaultAsync(p => p.Id == command.PostId, cancellationToken);

            if (post == null)
            {
                return Result.Failure<LikePostResult>("Post not found");
            }

            // Check if user already liked this post
            var existingReaction = await _context.Set<PostReaction>()
                .FirstOrDefaultAsync(
                    r => r.PostId == command.PostId && 
                         r.UserId == command.UserId &&
                         r.ReactionType == ReactionType.Like,
                    cancellationToken);

            bool isLiked;
            Guid likeId;

            if (existingReaction != null)
            {
                // User already liked it → Unlike
                likeId = existingReaction.Id;
                _context.Set<PostReaction>().Remove(existingReaction);
                post.DecrementLikes();
                isLiked = false;
                
                _logger.LogInformation(
                    "User {UserId} unliked post {PostId}", 
                    command.UserId, 
                    command.PostId);
            }
            else
            {
                // User hasn't liked it yet → Like
                var reaction = new PostReaction(
                    command.PostId, 
                    command.UserId, 
                    ReactionType.Like);
                
                likeId = reaction.Id;
                await _context.Set<PostReaction>().AddAsync(reaction, cancellationToken);
                post.IncrementLikes();
                isLiked = true;
                
                _logger.LogInformation(
                    "User {UserId} liked post {PostId}", 
                    command.UserId, 
                    command.PostId);
            }

            // Save changes atomically
            await _context.SaveChangesAsync(cancellationToken);

            var result = new LikePostResult
            {
                IsLiked = isLiked,
                TotalLikes = post.LikeCount
            };

            // Dispatch domain events for audit trail
            await DispatchDomainEventsAsync(isLiked, likeId, post, command, result, cancellationToken);

            return Result<LikePostResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error toggling like for post {PostId} by user {UserId}", 
                command.PostId, 
                command.UserId);
            
            return Result.Failure<LikePostResult>("Failed to toggle like");
        }
    }

    /// <summary>
    /// Dispatches domain events for audit trail and analytics
    /// </summary>
    private async Task DispatchDomainEventsAsync(
        bool isLiked,
        Guid likeId,
        Post post,
        LikePostCommand command,
        LikePostResult result,
        CancellationToken cancellationToken)
    {
        try
        {
            if (isLiked)
            {
                var createdEvent = new LikeCreatedEvent(
                    likeId,
                    post.Id,
                    "Post",
                    command.UserId,
                    result.TotalLikes);
                await _likeCreatedHandler.HandleAsync(createdEvent, cancellationToken);
            }
            else
            {
                var removedEvent = new LikeRemovedEvent(
                    likeId,
                    post.Id,
                    "Post",
                    command.UserId,
                    result.TotalLikes);
                await _likeRemovedHandler.HandleAsync(removedEvent, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching domain events for post like {LikeId}", likeId);
            // Don't throw - audit logging failure shouldn't break the like operation
        }
    }
}
