using FamilyRelocation.Domain.Enums;
using MediatR;

namespace FamilyRelocation.Application.Documents.Commands.CreateStageRequirement;

/// <summary>
/// Command to create a new stage transition requirement.
/// </summary>
public record CreateStageRequirementCommand(
    HousingSearchStage FromStage,
    HousingSearchStage ToStage,
    Guid DocumentTypeId,
    bool IsRequired
) : IRequest<Guid>;
