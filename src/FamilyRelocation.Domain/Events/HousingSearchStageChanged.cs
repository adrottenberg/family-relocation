using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Enums;

namespace FamilyRelocation.Domain.Events;

/// <summary>
/// Raised when a housing search's stage changes
/// </summary>
public sealed class HousingSearchStageChanged : IDomainEvent
{
    public Guid HousingSearchId { get; }
    public HousingSearchStage OldStage { get; }
    public HousingSearchStage NewStage { get; }
    public DateTime OccurredOn { get; }

    public HousingSearchStageChanged(Guid housingSearchId, HousingSearchStage oldStage, HousingSearchStage newStage)
    {
        HousingSearchId = housingSearchId;
        OldStage = oldStage;
        NewStage = newStage;
        OccurredOn = DateTime.UtcNow;
    }
}
