using CommunityCar.Domain.Entities.Dashboard.security;
using CommunityCar.Domain.Events.Community;
using CommunityCar.Domain.Interfaces;
using CommunityCar.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CommunityCar.Infrastructure.EventHandlers;

/// <summary>
/// Handles like domain events and creates audit log entries
/// Provides complete audit trail for analytics and compliance
/// </summary>
public class LikeAuditEventHandler :
    IDomainEventHandler<LikeCreatedEvent>,
    IDomainEventHandler<LikeRemovedEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LikeAuditEventHandler> _logger;

    public LikeAuditEventHandler(
        ApplicationDbContext context,
        ILogger<LikeAuditEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(LikeCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Like",
            EntityId = domainEvent.LikeId.ToString(),
            Action = "Created",
            Description = $"User liked {domainEvent.EntityType.ToLower()}",
            OldValues = null,
            NewValues = JsonSerializer.Serialize(new
            {
                LikeId = domainEvent.LikeId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType,
                NewLikeCount = domainEvent.NewLikeCount
            }),
            AffectedColumns = "LikeCount"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for LikeCreated: {EntityType} {EntityId} by user {UserId}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId);
    }

    public async Task HandleAsync(LikeRemovedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Like",
            EntityId = domainEvent.LikeId.ToString(),
            Action = "Deleted",
            Description = $"User unliked {domainEvent.EntityType.ToLower()}",
            OldValues = JsonSerializer.Serialize(new
            {
                Liked = true
            }),
            NewValues = JsonSerializer.Serialize(new
            {
                LikeId = domainEvent.LikeId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType,
                Liked = false,
                NewLikeCount = domainEvent.NewLikeCount
            }),
            AffectedColumns = "LikeCount"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for LikeRemoved: {EntityType} {EntityId} by user {UserId}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId);
    }
}
