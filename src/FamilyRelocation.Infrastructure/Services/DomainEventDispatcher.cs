using FamilyRelocation.Application.Common;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Common;
using MediatR;

namespace FamilyRelocation.Infrastructure.Services;

/// <summary>
/// Dispatches domain events via MediatR.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;

    public DomainEventDispatcher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
        var notification = Activator.CreateInstance(notificationType, domainEvent);

        if (notification != null)
        {
            await _publisher.Publish(notification, cancellationToken);
        }
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }
}
