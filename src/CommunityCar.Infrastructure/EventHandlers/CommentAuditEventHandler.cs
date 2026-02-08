using CommunityCar.Domain.Entities.Dashboard.security;
using CommunityCar.Domain.Events.Community;
using CommunityCar.Domain.Interfaces;
using CommunityCar.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CommunityCar.Infrastructure.EventHandlers;

/// <summary>
/// Handles comment domain events and creates audit log entries
/// Provides complete audit trail for analytics and compliance
/// </summary>
public class CommentAuditEventHandler :
    IDomainEventHandler<CommentCreatedEvent>,
    IDomainEventHandler<CommentUpdatedEvent>,
    IDomainEventHandler<CommentDeletedEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CommentAuditEventHandler> _logger;

    public CommentAuditEventHandler(
        ApplicationDbContext context,
        ILogger<CommentAuditEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(CommentCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Comment",
            EntityId = domainEvent.CommentId.ToString(),
            Action = "Created",
            Description = domainEvent.ParentCommentId.HasValue
                ? $"User replied to comment on {domainEvent.EntityType.ToLower()}"
                : $"User commented on {domainEvent.EntityType.ToLower()}",
            OldValues = null,
            NewValues = JsonSerializer.Serialize(new
            {
                CommentId = domainEvent.CommentId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType,
                Content = domainEvent.Content.Length > 100 
                    ? domainEvent.Content.Substring(0, 100) + "..." 
                    : domainEvent.Content,
                ParentCommentId = domainEvent.ParentCommentId,
                IsReply = domainEvent.ParentCommentId.HasValue,
                NewCommentCount = domainEvent.NewCommentCount
            }),
            AffectedColumns = "Content,CommentCount"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for CommentCreated: {EntityType} {EntityId} by user {UserId}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId);
    }

    public async Task HandleAsync(CommentUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Comment",
            EntityId = domainEvent.CommentId.ToString(),
            Action = "Updated",
            Description = $"User updated comment on {domainEvent.EntityType.ToLower()}",
            OldValues = JsonSerializer.Serialize(new
            {
                Content = domainEvent.OldContent.Length > 100 
                    ? domainEvent.OldContent.Substring(0, 100) + "..." 
                    : domainEvent.OldContent
            }),
            NewValues = JsonSerializer.Serialize(new
            {
                CommentId = domainEvent.CommentId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType,
                Content = domainEvent.NewContent.Length > 100 
                    ? domainEvent.NewContent.Substring(0, 100) + "..." 
                    : domainEvent.NewContent
            }),
            AffectedColumns = "Content,ModifiedAt"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for CommentUpdated: {EntityType} {EntityId} by user {UserId}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId);
    }

    public async Task HandleAsync(CommentDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Comment",
            EntityId = domainEvent.CommentId.ToString(),
            Action = "Deleted",
            Description = $"User deleted comment on {domainEvent.EntityType.ToLower()}",
            OldValues = JsonSerializer.Serialize(new
            {
                Content = domainEvent.Content.Length > 100 
                    ? domainEvent.Content.Substring(0, 100) + "..." 
                    : domainEvent.Content,
                IsDeleted = false
            }),
            NewValues = JsonSerializer.Serialize(new
            {
                CommentId = domainEvent.CommentId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType,
                IsDeleted = true,
                NewCommentCount = domainEvent.NewCommentCount
            }),
            AffectedColumns = "IsDeleted,DeletedAt,CommentCount"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for CommentDeleted: {EntityType} {EntityId} by user {UserId}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId);
    }
}
