using FamilyRelocation.Domain.Common;

namespace FamilyRelocation.Domain.Events;

/// <summary>
/// Raised when a new housing search is started
/// </summary>
public sealed class HousingSearchStarted : IDomainEvent
{
    public Guid HousingSearchId { get; }
    public Guid ApplicantId { get; }
    public DateTime OccurredOn { get; }

    public HousingSearchStarted(Guid housingSearchId, Guid applicantId)
    {
        HousingSearchId = housingSearchId;
        ApplicantId = applicantId;
        OccurredOn = DateTime.UtcNow;
    }
}
