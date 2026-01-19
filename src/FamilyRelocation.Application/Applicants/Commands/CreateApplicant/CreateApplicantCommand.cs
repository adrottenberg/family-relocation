using FamilyRelocation.Application.Applicants.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Applicants.Commands.CreateApplicant;

/// <summary>
/// Command to create a new applicant (family) and their housing search.
/// </summary>
public record CreateApplicantCommand : IRequest<CreateApplicantResponse>
{
    public required HusbandInfoDto Husband { get; init; }
    public SpouseInfoDto? Wife { get; init; }
    public AddressDto? Address { get; init; }
    public List<ChildDto>? Children { get; init; }
    public string? CurrentKehila { get; init; }
    public string? ShabbosShul { get; init; }

    /// <summary>
    /// Optional housing preferences for the initial submission.
    /// If not provided, default preferences will be used.
    /// </summary>
    public HousingPreferencesDto? HousingPreferences { get; init; }
}
