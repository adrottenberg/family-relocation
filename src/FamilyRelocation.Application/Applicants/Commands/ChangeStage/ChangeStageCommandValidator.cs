using FluentValidation;

namespace FamilyRelocation.Application.Applicants.Commands.ChangeStage;

/// <summary>
/// Validates the ChangeStageCommand before processing.
/// </summary>
public class ChangeStageCommandValidator : AbstractValidator<ChangeStageCommand>
{
    private static readonly string[] ValidStages =
        ["Searching", "Paused", "UnderContract", "Closed", "MovedIn"];

    /// <summary>
    /// Initializes validation rules for changing an applicant's stage.
    /// </summary>
    public ChangeStageCommandValidator()
    {
        RuleFor(x => x.ApplicantId)
            .NotEmpty().WithMessage("Applicant ID is required");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Stage change request is required");

        RuleFor(x => x.Request.NewStage)
            .NotEmpty().WithMessage("New stage is required")
            .Must(stage => ValidStages.Contains(stage))
            .WithMessage($"New stage must be one of: {string.Join(", ", ValidStages)}")
            .When(x => x.Request != null);

        // Contract is required when transitioning to UnderContract
        RuleFor(x => x.Request.Contract)
            .NotNull().WithMessage("Contract details are required when transitioning to UnderContract")
            .When(x => x.Request?.NewStage == "UnderContract");

        RuleFor(x => x.Request.Contract!.Price)
            .GreaterThan(0).WithMessage("Contract price must be greater than 0")
            .When(x => x.Request?.NewStage == "UnderContract" && x.Request.Contract != null);

        // Closing date is required when transitioning to Closed
        RuleFor(x => x.Request.ClosingDate)
            .NotNull().WithMessage("Closing date is required when transitioning to Closed")
            .When(x => x.Request?.NewStage == "Closed");

        // Move-in date is required when transitioning to MovedIn
        RuleFor(x => x.Request.MovedInDate)
            .NotNull().WithMessage("Move-in date is required when transitioning to MovedIn")
            .When(x => x.Request?.NewStage == "MovedIn");
    }
}
