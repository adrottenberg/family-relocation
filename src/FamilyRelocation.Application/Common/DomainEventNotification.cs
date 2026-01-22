using FamilyRelocation.Domain.Common;
using MediatR;

namespace FamilyRelocation.Application.Common;

/// <summary>
/// Wrapper that converts domain events to MediatR notifications.
/// </summary>
public class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; }

    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}
