using FluentValidation;

namespace FamilyRelocation.Application.Activities.Commands.LogActivity;

public class LogActivityCommandValidator : AbstractValidator<LogActivityCommand>
{
    private static readonly string[] ValidTypes = { "PhoneCall", "Note", "Email", "SMS" };
    private static readonly string[] ValidOutcomes = { "Connected", "Voicemail", "NoAnswer", "Busy", "LeftMessage" };
    private static readonly string[] ValidEntityTypes = { "Applicant", "Property", "HousingSearch" };

    public LogActivityCommandValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required")
            .Must(type => ValidEntityTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Entity type must be one of: {string.Join(", ", ValidEntityTypes)}");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Activity type is required")
            .Must(type => ValidTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Activity type must be one of: {string.Join(", ", ValidTypes)}");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        // Phone call specific validations
        When(x => x.Type.Equals("PhoneCall", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.DurationMinutes)
                .GreaterThanOrEqualTo(0).When(x => x.DurationMinutes.HasValue)
                .WithMessage("Duration must be a positive number");

            RuleFor(x => x.Outcome)
                .Must(outcome => string.IsNullOrEmpty(outcome) || ValidOutcomes.Contains(outcome, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Outcome must be one of: {string.Join(", ", ValidOutcomes)}");
        });

        // Follow-up validations
        When(x => x.CreateFollowUp, () =>
        {
            RuleFor(x => x.FollowUpDate)
                .NotNull().WithMessage("Follow-up date is required when creating a follow-up")
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Follow-up date cannot be in the past");
        });
    }
}
