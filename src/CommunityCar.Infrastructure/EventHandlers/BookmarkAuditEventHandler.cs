using CommunityCar.Domain.Entities.Dashboard.security;
using CommunityCar.Domain.Events.Community;
using CommunityCar.Domain.Interfaces;
using CommunityCar.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CommunityCar.Infrastructure.EventHandlers;

/// <summary>
/// Handles bookmark domain events and creates audit log entries
/// Provides complete audit trail for analytics and compliance
/// </summary>
public class BookmarkAuditEventHandler :
    IDomainEventHandler<BookmarkCreatedEvent>,
    IDomainEventHandler<BookmarkRemovedEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BookmarkAuditEventHandler> _logger;

    public BookmarkAuditEventHandler(
        ApplicationDbContext context,
        ILogger<BookmarkAuditEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(BookmarkCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Bookmark",
            EntityId = domainEvent.BookmarkId.ToString(),
            Action = "Created",
            Description = $"User bookmarked {domainEvent.EntityType.ToLower()}",
            OldValues = null,
            NewValues = JsonSerializer.Serialize(new
            {
                BookmarkId = domainEvent.BookmarkId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType
            }),
            AffectedColumns = "BookmarkCount"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for BookmarkCreated: {EntityType} {EntityId} by user {UserId}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId);
    }

    public async Task HandleAsync(BookmarkRemovedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Bookmark",
            EntityId = domainEvent.BookmarkId.ToString(),
            Action = "Deleted",
            Description = $"User removed bookmark from {domainEvent.EntityType.ToLower()}",
            OldValues = JsonSerializer.Serialize(new
            {
                Bookmarked = true
            }),
            NewValues = JsonSerializer.Serialize(new
            {
                BookmarkId = domainEvent.BookmarkId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType,
                Bookmarked = false
            }),
            AffectedColumns = "BookmarkCount"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for BookmarkRemoved: {EntityType} {EntityId} by user {UserId}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId);
    }
}
