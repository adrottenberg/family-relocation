using FluentValidation;

namespace FamilyRelocation.Application.Documents.Commands.CreateStageRequirement;

/// <summary>
/// Validates the CreateStageRequirementCommand before processing.
/// </summary>
public class CreateStageRequirementCommandValidator : AbstractValidator<CreateStageRequirementCommand>
{
    /// <summary>
    /// Initializes validation rules for creating a stage requirement.
    /// </summary>
    public CreateStageRequirementCommandValidator()
    {
        RuleFor(x => x.FromStage)
            .IsInEnum().WithMessage("From stage must be a valid housing search stage");

        RuleFor(x => x.ToStage)
            .IsInEnum().WithMessage("To stage must be a valid housing search stage")
            .NotEqual(x => x.FromStage).WithMessage("To stage must be different from From stage");

        RuleFor(x => x.DocumentTypeId)
            .NotEmpty().WithMessage("Document type ID is required");
    }
}
