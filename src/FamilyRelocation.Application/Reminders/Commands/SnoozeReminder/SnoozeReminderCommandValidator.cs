using FluentValidation;

namespace FamilyRelocation.Application.Reminders.Commands.SnoozeReminder;

/// <summary>
/// Validator for SnoozeReminderCommand.
/// </summary>
public class SnoozeReminderCommandValidator : AbstractValidator<SnoozeReminderCommand>
{
    public SnoozeReminderCommandValidator()
    {
        RuleFor(x => x.ReminderId)
            .NotEmpty().WithMessage("Reminder ID is required.");

        RuleFor(x => x.SnoozeUntil)
            .NotEmpty().WithMessage("Snooze until date is required.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Snooze date cannot be in the past.");
    }
}
