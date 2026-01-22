using FluentValidation;

namespace FamilyRelocation.Application.Applicants.Commands.UpdatePreferences;

/// <summary>
/// Validator for UpdatePreferencesCommand.
/// </summary>
public class UpdatePreferencesCommandValidator : AbstractValidator<UpdatePreferencesCommand>
{
    public UpdatePreferencesCommandValidator()
    {
        RuleFor(x => x.ApplicantId)
            .NotEmpty().WithMessage("Applicant ID is required.");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Preferences are required.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.BudgetAmount)
                .GreaterThan(0).WithMessage("Budget amount must be positive.")
                .LessThanOrEqualTo(50_000_000).WithMessage("Budget amount seems unrealistic.")
                .When(x => x.Request.BudgetAmount.HasValue);

            RuleFor(x => x.Request.MinBedrooms)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum bedrooms cannot be negative.")
                .LessThanOrEqualTo(20).WithMessage("Minimum bedrooms seems unrealistic.")
                .When(x => x.Request.MinBedrooms.HasValue);

            RuleFor(x => x.Request.MinBathrooms)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum bathrooms cannot be negative.")
                .LessThanOrEqualTo(20).WithMessage("Minimum bathrooms seems unrealistic.")
                .When(x => x.Request.MinBathrooms.HasValue);

            RuleFor(x => x.Request.MoveTimeline)
                .Must(BeValidMoveTimeline).WithMessage("Invalid move timeline value.")
                .When(x => !string.IsNullOrEmpty(x.Request.MoveTimeline));
        });
    }

    private static bool BeValidMoveTimeline(string? timeline)
    {
        if (string.IsNullOrEmpty(timeline)) return true;

        var validTimelines = new[]
        {
            "Immediate", "ShortTerm", "MediumTerm", "LongTerm",
            "Extended", "Flexible", "NotSure", "Never"
        };

        return validTimelines.Contains(timeline, StringComparer.OrdinalIgnoreCase);
    }
}
