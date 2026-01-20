using MediatR;

namespace FamilyRelocation.Application.Applicants.Commands.DeleteApplicant;

/// <summary>
/// Command to soft delete an applicant.
/// Sets IsDeleted = true rather than removing from database.
/// </summary>
public record DeleteApplicantCommand(Guid ApplicantId) : IRequest;
