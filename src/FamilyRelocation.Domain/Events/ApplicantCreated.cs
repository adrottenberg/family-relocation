using FamilyRelocation.Domain.Common;

namespace FamilyRelocation.Domain.Events;

/// <summary>
/// Raised when a new applicant is created
/// </summary>
public sealed class ApplicantCreated : IDomainEvent
{
    public Guid ApplicantId { get; }
    public DateTime OccurredOn { get; }

    public ApplicantCreated(Guid applicantId)
    {
        ApplicantId = applicantId;
        OccurredOn = DateTime.UtcNow;
    }
}
