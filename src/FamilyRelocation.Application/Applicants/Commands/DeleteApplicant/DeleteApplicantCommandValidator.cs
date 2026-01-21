using FluentValidation;

namespace FamilyRelocation.Application.Applicants.Commands.DeleteApplicant;

/// <summary>
/// Validates the DeleteApplicantCommand before processing.
/// </summary>
public class DeleteApplicantCommandValidator : AbstractValidator<DeleteApplicantCommand>
{
    /// <summary>
    /// Initializes validation rules for deleting an applicant.
    /// </summary>
    public DeleteApplicantCommandValidator()
    {
        RuleFor(x => x.ApplicantId)
            .NotEmpty().WithMessage("Applicant ID is required");
    }
}
