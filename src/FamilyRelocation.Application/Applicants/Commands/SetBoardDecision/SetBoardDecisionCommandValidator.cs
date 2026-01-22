using FluentValidation;

namespace FamilyRelocation.Application.Applicants.Commands.SetBoardDecision;

/// <summary>
/// Validator for SetBoardDecisionCommand.
/// </summary>
public class SetBoardDecisionCommandValidator : AbstractValidator<SetBoardDecisionCommand>
{
    public SetBoardDecisionCommandValidator()
    {
        RuleFor(x => x.ApplicantId)
            .NotEmpty().WithMessage("Applicant ID is required.");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request body is required.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Decision)
                .IsInEnum().WithMessage("Invalid board decision value.");

            RuleFor(x => x.Request.Notes)
                .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.")
                .When(x => x.Request.Notes != null);

            RuleFor(x => x.Request.ReviewDate)
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Review date cannot be in the future.")
                .When(x => x.Request.ReviewDate.HasValue);
        });
    }
}
