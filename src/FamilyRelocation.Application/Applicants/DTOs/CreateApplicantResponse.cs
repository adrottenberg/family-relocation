namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// Response from creating an applicant, includes both Applicant and HousingSearch IDs.
/// </summary>
public class CreateApplicantResponse
{
    /// <summary>
    /// The ID of the created applicant.
    /// </summary>
    public required Guid ApplicantId { get; init; }

    /// <summary>
    /// The ID of the automatically created housing search.
    /// </summary>
    public required Guid HousingSearchId { get; init; }

    /// <summary>
    /// The full applicant details.
    /// </summary>
    public required ApplicantDto Applicant { get; init; }
}
