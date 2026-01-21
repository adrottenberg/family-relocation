using FamilyRelocation.Domain.Enums;
using MediatR;

namespace FamilyRelocation.Application.Applicants.Commands.SetBoardDecision;

/// <summary>
/// Command to set the board's decision on an applicant.
/// </summary>
public record SetBoardDecisionCommand(
    Guid ApplicantId,
    SetBoardDecisionRequest Request) : IRequest<SetBoardDecisionResponse>;

/// <summary>
/// Request body for setting board decision.
/// </summary>
public class SetBoardDecisionRequest
{
    /// <summary>
    /// The board's decision. Required.
    /// </summary>
    public required BoardDecision Decision { get; init; }

    /// <summary>
    /// Date of the board review. Defaults to today if not specified.
    /// </summary>
    public DateTime? ReviewDate { get; init; }

    /// <summary>
    /// Optional notes from the board review.
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Response returned after setting board decision.
/// </summary>
public class SetBoardDecisionResponse
{
    /// <summary>
    /// The applicant ID.
    /// </summary>
    public required Guid ApplicantId { get; init; }

    /// <summary>
    /// The housing search ID (only set if approved).
    /// </summary>
    public Guid? HousingSearchId { get; init; }

    /// <summary>
    /// The board review details.
    /// </summary>
    public required BoardReviewDto BoardReview { get; init; }

    /// <summary>
    /// The previous housing search stage before this decision.
    /// </summary>
    public required string PreviousStage { get; init; }

    /// <summary>
    /// The new housing search stage after this decision.
    /// </summary>
    public required string NewStage { get; init; }

    /// <summary>
    /// Guidance message for next steps.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Next steps information (for Approved decisions).
    /// </summary>
    public NextStepsInfo? NextSteps { get; init; }
}

/// <summary>
/// Information about next steps after board approval.
/// </summary>
public class NextStepsInfo
{
    /// <summary>
    /// Whether the broker agreement has been signed.
    /// </summary>
    public required bool BrokerAgreementSigned { get; init; }

    /// <summary>
    /// Whether the community takanos have been signed.
    /// </summary>
    public required bool CommunityTakanosSigned { get; init; }

    /// <summary>
    /// Whether all required agreements are signed and applicant can start house hunting.
    /// </summary>
    public required bool ReadyForHouseHunting { get; init; }
}

/// <summary>
/// DTO for board review information.
/// </summary>
public class BoardReviewDto
{
    /// <summary>
    /// The board's decision.
    /// </summary>
    public required string Decision { get; init; }

    /// <summary>
    /// Date of the review.
    /// </summary>
    public required DateTime ReviewDate { get; init; }

    /// <summary>
    /// Notes from the review.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// User ID of the reviewer.
    /// </summary>
    public required Guid ReviewedBy { get; init; }
}
