using MediatR;
using FamilyRelocation.Application.Applicants.DTOs;

namespace FamilyRelocation.Application.Applicants.Queries.GetApplicantById;

/// <summary>
/// Query to get an applicant by ID
/// </summary>
public record GetApplicantByIdQuery(Guid Id) : IRequest<ApplicantDto?>;
