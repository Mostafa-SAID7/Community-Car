namespace CommunityCar.Domain.Base.Interfaces;

/// <summary>
/// Interface for domain events.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
