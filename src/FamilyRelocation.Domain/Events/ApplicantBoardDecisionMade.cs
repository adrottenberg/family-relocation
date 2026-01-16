using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Enums;

namespace FamilyRelocation.Domain.Events;

/// <summary>
/// Raised when an applicant receives a board decision (approved, rejected, or deferred)
/// </summary>
public sealed class ApplicantBoardDecisionMade : IDomainEvent
{
    public Guid ApplicantId { get; }
    public BoardDecision Decision { get; }
    public Guid ReviewedByUserId { get; }
    public DateTime OccurredOn { get; }

    public ApplicantBoardDecisionMade(Guid applicantId, BoardDecision decision, Guid reviewedByUserId)
    {
        ApplicantId = applicantId;
        Decision = decision;
        ReviewedByUserId = reviewedByUserId;
        OccurredOn = DateTime.UtcNow;
    }
}
