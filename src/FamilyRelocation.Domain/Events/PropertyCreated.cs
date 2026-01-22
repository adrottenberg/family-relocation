using FamilyRelocation.Domain.Common;

namespace FamilyRelocation.Domain.Events;

/// <summary>
/// Raised when a new property is created.
/// Used to trigger automatic matching against active housing searches.
/// </summary>
public sealed class PropertyCreated : IDomainEvent
{
    public Guid PropertyId { get; }
    public DateTime OccurredOn { get; }

    public PropertyCreated(Guid propertyId)
    {
        PropertyId = propertyId;
        OccurredOn = DateTime.UtcNow;
    }
}
