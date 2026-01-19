using FamilyRelocation.Application.Applicants.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Applicants.Commands.CreateApplicant;

/// <summary>
/// Command to create a new applicant (family) and their housing search.
/// </summary>
public record CreateApplicantCommand : IRequest<CreateApplicantResponse>
{
    /// <summary>
    /// Required information about the husband/primary applicant.
    /// </summary>
    public required HusbandInfoDto Husband { get; init; }

    /// <summary>
    /// Optional information about the wife/spouse.
    /// </summary>
    public SpouseInfoDto? Wife { get; init; }

    /// <summary>
    /// Current residential address of the family.
    /// </summary>
    public AddressDto? Address { get; init; }

    /// <summary>
    /// List of children in the family.
    /// </summary>
    public List<ChildDto>? Children { get; init; }

    /// <summary>
    /// Current kehila/community affiliation.
    /// </summary>
    public string? CurrentKehila { get; init; }

    /// <summary>
    /// Shul attended on Shabbos.
    /// </summary>
    public string? ShabbosShul { get; init; }

    /// <summary>
    /// Optional housing preferences for the initial submission.
    /// If not provided, default preferences will be used.
    /// </summary>
    public HousingPreferencesDto? HousingPreferences { get; init; }
}
