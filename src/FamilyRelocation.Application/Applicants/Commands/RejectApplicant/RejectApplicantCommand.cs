using FamilyRelocation.Domain.Enums;
using MediatR;

namespace FamilyRelocation.Application.Applicants.Commands.RejectApplicant;

/// <summary>
/// Command to reject an applicant (transition to Rejected stage).
/// Requires board decision to be Rejected first.
/// </summary>
public record RejectApplicantCommand(
    Guid ApplicantId,
    RejectApplicantRequest? Request) : IRequest<RejectApplicantResponse>;

/// <summary>
/// Optional request body for rejecting an applicant.
/// </summary>
public class RejectApplicantRequest
{
    /// <summary>
    /// Reason for rejection (optional, for records).
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Whether to send notification email (for future use).
    /// </summary>
    public bool SendNotification { get; init; }
}

/// <summary>
/// Response returned after rejecting an applicant.
/// </summary>
public class RejectApplicantResponse
{
    /// <summary>
    /// The applicant ID.
    /// </summary>
    public required Guid ApplicantId { get; init; }

    /// <summary>
    /// The housing search ID.
    /// </summary>
    public required Guid HousingSearchId { get; init; }

    /// <summary>
    /// The previous stage before rejection.
    /// </summary>
    public required string PreviousStage { get; init; }

    /// <summary>
    /// The new stage (Rejected).
    /// </summary>
    public required string NewStage { get; init; }

    /// <summary>
    /// When the rejection was recorded.
    /// </summary>
    public required DateTime RejectedAt { get; init; }

    /// <summary>
    /// User who recorded the rejection.
    /// </summary>
    public required Guid RejectedBy { get; init; }

    /// <summary>
    /// The rejection reason if provided.
    /// </summary>
    public string? Reason { get; init; }
}
