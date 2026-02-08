using CommunityCar.Domain.Entities.Dashboard.security;
using CommunityCar.Domain.Events.Community;
using CommunityCar.Domain.Interfaces;
using CommunityCar.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CommunityCar.Infrastructure.EventHandlers;

/// <summary>
/// Handles vote domain events and creates audit log entries
/// Provides complete audit trail for analytics and compliance
/// </summary>
public class VoteAuditEventHandler :
    IDomainEventHandler<VoteCreatedEvent>,
    IDomainEventHandler<VoteChangedEvent>,
    IDomainEventHandler<VoteRemovedEvent>,
    IDomainEventHandler<VoteResurrectedEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VoteAuditEventHandler> _logger;

    public VoteAuditEventHandler(
        ApplicationDbContext context,
        ILogger<VoteAuditEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(VoteCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Vote",
            EntityId = domainEvent.VoteId.ToString(),
            Action = "Created",
            Description = $"User voted {(domainEvent.IsUpvote ? "up" : "down")} on {domainEvent.EntityType.ToLower()}",
            OldValues = null,
            NewValues = JsonSerializer.Serialize(new
            {
                VoteId = domainEvent.VoteId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType,
                IsUpvote = domainEvent.IsUpvote,
                ScoreDelta = domainEvent.ScoreDelta,
                NewScore = domainEvent.NewScore
            }),
            AffectedColumns = "IsUpvote,VoteCount"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for VoteCreated: {EntityType} {EntityId} by user {UserId}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId);
    }

    public async Task HandleAsync(VoteChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Vote",
            EntityId = domainEvent.VoteId.ToString(),
            Action = "Updated",
            Description = $"User changed vote from {(domainEvent.OldIsUpvote ? "up" : "down")} to {(domainEvent.NewIsUpvote ? "up" : "down")} on {domainEvent.EntityType.ToLower()}",
            OldValues = JsonSerializer.Serialize(new
            {
                IsUpvote = domainEvent.OldIsUpvote
            }),
            NewValues = JsonSerializer.Serialize(new
            {
                VoteId = domainEvent.VoteId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType,
                IsUpvote = domainEvent.NewIsUpvote,
                ScoreDelta = domainEvent.ScoreDelta,
                NewScore = domainEvent.NewScore
            }),
            AffectedColumns = "IsUpvote,VoteCount,ModifiedAt"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for VoteChanged: {EntityType} {EntityId} by user {UserId}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId);
    }

    public async Task HandleAsync(VoteRemovedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Vote",
            EntityId = domainEvent.VoteId.ToString(),
            Action = "Deleted",
            Description = $"User removed {(domainEvent.WasUpvote ? "up" : "down")}vote from {domainEvent.EntityType.ToLower()}",
            OldValues = JsonSerializer.Serialize(new
            {
                IsUpvote = domainEvent.WasUpvote,
                IsDeleted = false
            }),
            NewValues = JsonSerializer.Serialize(new
            {
                VoteId = domainEvent.VoteId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType,
                IsDeleted = true,
                ScoreDelta = domainEvent.ScoreDelta,
                NewScore = domainEvent.NewScore
            }),
            AffectedColumns = "IsDeleted,DeletedAt,VoteCount"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for VoteRemoved: {EntityType} {EntityId} by user {UserId}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId);
    }

    public async Task HandleAsync(VoteResurrectedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Vote",
            EntityId = domainEvent.VoteId.ToString(),
            Action = "Updated",
            Description = $"User resurrected {(domainEvent.IsUpvote ? "up" : "down")}vote on {domainEvent.EntityType.ToLower()}" +
                         (domainEvent.DirectionChanged ? " with direction change" : ""),
            OldValues = JsonSerializer.Serialize(new
            {
                IsDeleted = true
            }),
            NewValues = JsonSerializer.Serialize(new
            {
                VoteId = domainEvent.VoteId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType,
                IsUpvote = domainEvent.IsUpvote,
                IsDeleted = false,
                DirectionChanged = domainEvent.DirectionChanged,
                ScoreDelta = domainEvent.ScoreDelta,
                NewScore = domainEvent.NewScore
            }),
            AffectedColumns = domainEvent.DirectionChanged 
                ? "IsDeleted,DeletedAt,IsUpvote,ModifiedAt,VoteCount" 
                : "IsDeleted,DeletedAt,ModifiedAt,VoteCount"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for VoteResurrected: {EntityType} {EntityId} by user {UserId}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId);
    }
}
