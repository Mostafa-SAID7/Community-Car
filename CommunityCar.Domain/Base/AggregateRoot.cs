using System.ComponentModel.DataAnnotations.Schema;
using CommunityCar.Domain.Base.Interfaces;

namespace CommunityCar.Domain.Base;

/// <summary>
/// Base class for Aggregate Roots in the domain.
/// Aggregate roots are the entry point to an aggregate and manage domain events.
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new DomainException(rule.Message);
        }
    }
}
