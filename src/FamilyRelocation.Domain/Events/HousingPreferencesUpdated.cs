using FamilyRelocation.Domain.Common;

namespace FamilyRelocation.Domain.Events;

/// <summary>
/// Raised when applicant's housing preferences are updated
/// </summary>
public sealed class HousingPreferencesUpdated : IDomainEvent
{
    public Guid ApplicantId { get; }
    public DateTime OccurredOn { get; }

    public HousingPreferencesUpdated(Guid applicantId)
    {
        ApplicantId = applicantId;
        OccurredOn = DateTime.UtcNow;
    }
}
