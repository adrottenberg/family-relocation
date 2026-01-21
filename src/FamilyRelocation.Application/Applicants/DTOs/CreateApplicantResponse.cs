namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// Response from creating an applicant.
/// </summary>
public class CreateApplicantResponse
{
    /// <summary>
    /// The ID of the created applicant.
    /// </summary>
    public required Guid ApplicantId { get; init; }

    /// <summary>
    /// The ID of the housing search (null until applicant is approved by board).
    /// </summary>
    public Guid? HousingSearchId { get; init; }

    /// <summary>
    /// The full applicant details.
    /// </summary>
    public required ApplicantDto Applicant { get; init; }
}
