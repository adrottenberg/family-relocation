using MediatR;

namespace FamilyRelocation.Application.Documents.Commands.DeleteStageRequirement;

/// <summary>
/// Command to delete a stage transition requirement.
/// </summary>
public record DeleteStageRequirementCommand(Guid Id) : IRequest<bool>;
