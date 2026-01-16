using FamilyRelocation.Domain.Common;

namespace FamilyRelocation.Domain.Events;

/// <summary>
/// Raised when a new applicant is created
/// </summary>
public sealed class ApplicantCreated : IDomainEvent
{
    public Guid ApplicantId { get; }
    public Guid? ProspectId { get; }
    public DateTime OccurredOn { get; }

    public ApplicantCreated(Guid applicantId, Guid? prospectId = null)
    {
        ApplicantId = applicantId;
        ProspectId = prospectId;
        OccurredOn = DateTime.UtcNow;
    }
}
