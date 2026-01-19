using FamilyRelocation.Application.Applicants.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Applicants.Commands.UpdatePreferences;

/// <summary>
/// Command to update housing preferences for an applicant's housing search.
/// </summary>
/// <param name="ApplicantId">The applicant ID.</param>
/// <param name="Request">The preferences update request.</param>
public record UpdatePreferencesCommand(
    Guid ApplicantId,
    HousingPreferencesDto Request) : IRequest<UpdatePreferencesResponse>;

/// <summary>
/// Response returned after updating housing preferences.
/// </summary>
public class UpdatePreferencesResponse
{
    /// <summary>
    /// The updated housing preferences.
    /// </summary>
    public required HousingPreferencesDto Preferences { get; init; }

    /// <summary>
    /// When the preferences were last modified.
    /// </summary>
    public required DateTime ModifiedDate { get; init; }
}
