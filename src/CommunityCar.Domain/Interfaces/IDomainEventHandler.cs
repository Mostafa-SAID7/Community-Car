using CommunityCar.Domain.Base.Interfaces;

namespace CommunityCar.Domain.Interfaces;

/// <summary>
/// Interface for domain event handlers
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
