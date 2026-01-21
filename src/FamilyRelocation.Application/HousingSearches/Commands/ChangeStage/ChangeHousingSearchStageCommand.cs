using MediatR;

namespace FamilyRelocation.Application.HousingSearches.Commands.ChangeStage;

/// <summary>
/// Command to change a housing search's stage.
/// Required fields depend on the target stage.
/// </summary>
public record ChangeHousingSearchStageCommand(
    Guid HousingSearchId,
    ChangeHousingSearchStageRequest Request
) : IRequest<ChangeHousingSearchStageResponse>;

/// <summary>
/// Request body for changing housing search stage.
/// </summary>
public class ChangeHousingSearchStageRequest
{
    /// <summary>
    /// Target stage: AwaitingAgreements, Searching, Paused, UnderContract, Closed, MovedIn.
    /// </summary>
    public required string NewStage { get; init; }

    /// <summary>
    /// Reason for pause or contract falling through.
    /// Required for: Paused. Optional for returning to Searching from UnderContract/Closed.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Contract details. Required when transitioning to UnderContract.
    /// </summary>
    public ContractRequest? Contract { get; init; }

    /// <summary>
    /// Closing date. Required when transitioning to Closed.
    /// </summary>
    public DateTime? ClosingDate { get; init; }

    /// <summary>
    /// Move-in date. Required when transitioning to MovedIn.
    /// </summary>
    public DateTime? MovedInDate { get; init; }
}

/// <summary>
/// Contract details for putting a property under contract.
/// </summary>
public class ContractRequest
{
    /// <summary>
    /// Property ID (optional - for linking to Property entity).
    /// </summary>
    public Guid? PropertyId { get; init; }

    /// <summary>
    /// Contract price in dollars.
    /// </summary>
    public required decimal Price { get; init; }

    /// <summary>
    /// Expected closing date.
    /// </summary>
    public DateTime? ExpectedClosingDate { get; init; }
}

/// <summary>
/// Response after changing housing search stage.
/// </summary>
public class ChangeHousingSearchStageResponse
{
    /// <summary>
    /// The housing search ID.
    /// </summary>
    public required Guid HousingSearchId { get; init; }

    /// <summary>
    /// The new stage after transition.
    /// </summary>
    public required string Stage { get; init; }

    /// <summary>
    /// When the stage was changed.
    /// </summary>
    public required DateTime StageChangedDate { get; init; }
}
