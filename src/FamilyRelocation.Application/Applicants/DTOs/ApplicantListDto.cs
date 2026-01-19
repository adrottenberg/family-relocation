namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// Lightweight DTO for applicant list views.
/// Contains summary info, not full details.
/// </summary>
public class ApplicantListDto
{
    /// <summary>
    /// Unique identifier for the applicant.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Full name of the husband (first + last).
    /// </summary>
    public required string HusbandFullName { get; init; }

    /// <summary>
    /// Wife's maiden name for identification.
    /// </summary>
    public string? WifeMaidenName { get; init; }

    /// <summary>
    /// Husband's email address.
    /// </summary>
    public string? HusbandEmail { get; init; }

    /// <summary>
    /// Husband's primary phone number.
    /// </summary>
    public string? HusbandPhone { get; init; }

    /// <summary>
    /// Board decision status (Pending, Approved, Rejected, Deferred).
    /// </summary>
    public string? BoardDecision { get; init; }

    /// <summary>
    /// Date the applicant record was created.
    /// </summary>
    public DateTime CreatedDate { get; init; }

    /// <summary>
    /// Housing search stage (Submitted, HouseHunting, UnderContract, Closed, etc).
    /// Null if no housing search exists.
    /// </summary>
    public string? Stage { get; init; }

    /// <summary>
    /// Housing search ID if one exists.
    /// </summary>
    public Guid? HousingSearchId { get; init; }
}
