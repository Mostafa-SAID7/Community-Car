using CommunityCar.Domain.Entities.Dashboard.security;
using CommunityCar.Domain.Events.Community;
using CommunityCar.Domain.Interfaces;
using CommunityCar.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CommunityCar.Infrastructure.EventHandlers;

/// <summary>
/// Handles rating domain events and creates audit log entries
/// Provides complete audit trail for analytics and compliance
/// </summary>
public class RatingAuditEventHandler :
    IDomainEventHandler<RatingCreatedEvent>,
    IDomainEventHandler<RatingUpdatedEvent>,
    IDomainEventHandler<RatingRemovedEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RatingAuditEventHandler> _logger;

    public RatingAuditEventHandler(
        ApplicationDbContext context,
        ILogger<RatingAuditEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(RatingCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Rating",
            EntityId = domainEvent.RatingId.ToString(),
            Action = "Created",
            Description = $"User rated {domainEvent.EntityType.ToLower()} with {domainEvent.RatingValue} stars",
            OldValues = null,
            NewValues = JsonSerializer.Serialize(new
            {
                RatingId = domainEvent.RatingId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType,
                RatingValue = domainEvent.RatingValue,
                NewAverageRating = domainEvent.NewAverageRating,
                NewRatingCount = domainEvent.NewRatingCount
            }),
            AffectedColumns = "RatingValue,AverageRating,RatingCount"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for RatingCreated: {EntityType} {EntityId} by user {UserId}, Rating={Rating}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId,
            domainEvent.RatingValue);
    }

    public async Task HandleAsync(RatingUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Rating",
            EntityId = domainEvent.RatingId.ToString(),
            Action = "Updated",
            Description = $"User changed rating on {domainEvent.EntityType.ToLower()} from {domainEvent.OldRatingValue} to {domainEvent.NewRatingValue} stars",
            OldValues = JsonSerializer.Serialize(new
            {
                RatingValue = domainEvent.OldRatingValue
            }),
            NewValues = JsonSerializer.Serialize(new
            {
                RatingId = domainEvent.RatingId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType,
                RatingValue = domainEvent.NewRatingValue,
                NewAverageRating = domainEvent.NewAverageRating
            }),
            AffectedColumns = "RatingValue,AverageRating,ModifiedAt"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for RatingUpdated: {EntityType} {EntityId} by user {UserId}, Old={Old}, New={New}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId,
            domainEvent.OldRatingValue,
            domainEvent.NewRatingValue);
    }

    public async Task HandleAsync(RatingRemovedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = domainEvent.UserId,
            EntityName = $"{domainEvent.EntityType}Rating",
            EntityId = domainEvent.RatingId.ToString(),
            Action = "Deleted",
            Description = $"User removed {domainEvent.RatingValue}-star rating from {domainEvent.EntityType.ToLower()}",
            OldValues = JsonSerializer.Serialize(new
            {
                RatingValue = domainEvent.RatingValue
            }),
            NewValues = JsonSerializer.Serialize(new
            {
                RatingId = domainEvent.RatingId,
                EntityId = domainEvent.EntityId,
                EntityType = domainEvent.EntityType,
                NewAverageRating = domainEvent.NewAverageRating,
                NewRatingCount = domainEvent.NewRatingCount
            }),
            AffectedColumns = "AverageRating,RatingCount"
        };

        await _context.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit log created for RatingRemoved: {EntityType} {EntityId} by user {UserId}",
            domainEvent.EntityType,
            domainEvent.EntityId,
            domainEvent.UserId);
    }
}
