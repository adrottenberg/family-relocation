using FamilyRelocation.Domain.Common;

namespace FamilyRelocation.Application.Common.Interfaces;

/// <summary>
/// Dispatches domain events to their handlers.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
