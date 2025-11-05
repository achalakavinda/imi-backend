using System.ComponentModel.DataAnnotations.Schema;

namespace MigratingAssistant.Domain.Common;

public abstract class BaseEntity<TId>
{
    // Generic Id to allow different key types (int, long, Guid, etc.)
    public TId Id { get; set; } = default!;

    private readonly List<BaseEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

// Preserve existing non-generic alias for backward compatibility (int keys)
public abstract class BaseEntity : BaseEntity<int>
{
}
