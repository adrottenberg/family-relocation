using FamilyRelocation.Application.Documents.DTOs;
using FamilyRelocation.Domain.Enums;
using MediatR;

namespace FamilyRelocation.Application.Documents.Queries.GetStageRequirements;

/// <summary>
/// Query to get document requirements for a stage transition.
/// </summary>
/// <param name="FromStage">The stage transitioning from.</param>
/// <param name="ToStage">The stage transitioning to.</param>
/// <param name="ApplicantId">Optional applicant ID to check which documents are already uploaded.</param>
public record GetStageRequirementsQuery(
    HousingSearchStage FromStage,
    HousingSearchStage ToStage,
    Guid? ApplicantId = null) : IRequest<StageTransitionRequirementsDto>;
