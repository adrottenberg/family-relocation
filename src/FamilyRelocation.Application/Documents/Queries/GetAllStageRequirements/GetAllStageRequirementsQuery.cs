using FamilyRelocation.Application.Documents.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Documents.Queries.GetAllStageRequirements;

/// <summary>
/// Query to get all stage transition requirements.
/// </summary>
public record GetAllStageRequirementsQuery : IRequest<List<StageTransitionRequirementDto>>;
