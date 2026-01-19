using FamilyRelocation.Application.Applicants.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Applicants.Commands.UpdateApplicant;

/// <summary>
/// Command to update an existing applicant's basic information.
/// Cannot update: board decision, created date, applicant ID.
/// </summary>
public record UpdateApplicantCommand : IRequest<ApplicantDto>
{
    /// <summary>
    /// Unique identifier of the applicant to update.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Updated information about the husband/primary applicant.
    /// </summary>
    public required HusbandInfoDto Husband { get; init; }

    /// <summary>
    /// Updated information about the wife/spouse.
    /// </summary>
    public SpouseInfoDto? Wife { get; init; }

    /// <summary>
    /// Updated residential address of the family.
    /// </summary>
    public AddressDto? Address { get; init; }

    /// <summary>
    /// Updated list of children in the family.
    /// </summary>
    public List<ChildDto>? Children { get; init; }

    /// <summary>
    /// Updated kehila/community affiliation.
    /// </summary>
    public string? CurrentKehila { get; init; }

    /// <summary>
    /// Updated Shabbos shul.
    /// </summary>
    public string? ShabbosShul { get; init; }
}
