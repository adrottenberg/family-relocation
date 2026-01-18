using FamilyRelocation.Application.Applicants.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Applicants.Commands.UpdateApplicant;

/// <summary>
/// Command to update an existing applicant's basic information.
/// Cannot update: board decision, created date, applicant ID.
/// </summary>
public record UpdateApplicantCommand : IRequest<ApplicantDto>
{
    public required Guid Id { get; init; }
    public required HusbandInfoDto Husband { get; init; }
    public SpouseInfoDto? Wife { get; init; }
    public AddressDto? Address { get; init; }
    public List<ChildDto>? Children { get; init; }
    public string? CurrentKehila { get; init; }
    public string? ShabbosShul { get; init; }
}
