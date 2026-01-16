using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Enums;

namespace FamilyRelocation.Domain.Events;

/// <summary>
/// Raised when an applicant receives a board decision
/// </summary>
public sealed class ApplicantApprovedByBoard : IDomainEvent
{
    public Guid ApplicantId { get; }
    public BoardDecision Decision { get; }
    public Guid ReviewedByUserId { get; }
    public DateTime OccurredOn { get; }

    public ApplicantApprovedByBoard(Guid applicantId, BoardDecision decision, Guid reviewedByUserId)
    {
        ApplicantId = applicantId;
        Decision = decision;
        ReviewedByUserId = reviewedByUserId;
        OccurredOn = DateTime.UtcNow;
    }
}
