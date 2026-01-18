namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// Lightweight DTO for applicant list views.
/// Contains summary info, not full details.
/// </summary>
public class ApplicantListDto
{
    public required Guid Id { get; init; }
    public required string FamilyName { get; init; }
    public string? HusbandEmail { get; init; }
    public string? HusbandPhone { get; init; }
    public string? City { get; init; }
    public int NumberOfChildren { get; init; }
    public string? BoardDecision { get; init; }
    public bool IsPendingBoardReview { get; init; }
    public bool IsSelfSubmitted { get; init; }
    public DateTime CreatedDate { get; init; }
}
