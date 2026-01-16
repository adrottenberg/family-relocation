using FamilyRelocation.Application.Applicants.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Applicants.Commands.CreateApplicant;

/// <summary>
/// Command to create a new applicant (family)
/// </summary>
public record CreateApplicantCommand : IRequest<ApplicantDto>
{
    public required HusbandInfoDto Husband { get; init; }
    public SpouseInfoDto? Wife { get; init; }
    public AddressDto? Address { get; init; }
    public List<ChildDto>? Children { get; init; }
    public string? CurrentKehila { get; init; }
    public string? ShabbosShul { get; init; }
}
