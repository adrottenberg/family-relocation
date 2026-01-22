using FamilyRelocation.Domain.Common;

namespace FamilyRelocation.Domain.Events;

/// <summary>
/// Raised when a new shul is created.
/// Used to trigger walking distance calculation from all active properties.
/// </summary>
public sealed class ShulCreated : IDomainEvent
{
    public Guid ShulId { get; }
    public DateTime OccurredOn { get; }

    public ShulCreated(Guid shulId)
    {
        ShulId = shulId;
        OccurredOn = DateTime.UtcNow;
    }
}
