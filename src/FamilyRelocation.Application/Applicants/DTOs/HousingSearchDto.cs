namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// Housing search details for an applicant.
/// </summary>
public record HousingSearchDto
{
    /// <summary>
    /// Unique identifier for the housing search.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Current stage of the housing search.
    /// </summary>
    public required string Stage { get; init; }

    /// <summary>
    /// Date when the stage was last changed.
    /// </summary>
    public required DateTime StageChangedDate { get; init; }

    /// <summary>
    /// Housing preferences.
    /// </summary>
    public HousingPreferencesDto? Preferences { get; init; }

    /// <summary>
    /// Current contract details (if under contract).
    /// </summary>
    public ContractDto? CurrentContract { get; init; }

    /// <summary>
    /// Number of contracts that have failed.
    /// </summary>
    public required int FailedContractCount { get; init; }

    /// <summary>
    /// Notes about the housing search.
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Contract details for a property.
/// </summary>
public record ContractDto
{
    /// <summary>
    /// Property identifier.
    /// </summary>
    public Guid? PropertyId { get; init; }

    /// <summary>
    /// Contract price amount.
    /// </summary>
    public required decimal Price { get; init; }

    /// <summary>
    /// Date the contract was signed.
    /// </summary>
    public required DateTime ContractDate { get; init; }

    /// <summary>
    /// Expected closing date.
    /// </summary>
    public DateTime? ExpectedClosingDate { get; init; }

    /// <summary>
    /// Actual closing date (if closed).
    /// </summary>
    public DateTime? ActualClosingDate { get; init; }
}
