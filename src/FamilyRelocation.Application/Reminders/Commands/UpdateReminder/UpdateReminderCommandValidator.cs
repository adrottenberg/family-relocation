using FluentValidation;

namespace FamilyRelocation.Application.Reminders.Commands.UpdateReminder;

/// <summary>
/// Validator for UpdateReminderCommand.
/// </summary>
public class UpdateReminderCommandValidator : AbstractValidator<UpdateReminderCommand>
{
    public UpdateReminderCommandValidator()
    {
        RuleFor(x => x.ReminderId)
            .NotEmpty().WithMessage("Reminder ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title cannot be empty.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.")
            .When(x => x.Title != null);

        RuleFor(x => x.DueDateTime)
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-5)).WithMessage("Due date/time cannot be in the past.")
            .When(x => x.DueDateTime.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.")
            .When(x => x.Notes != null);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value.")
            .When(x => x.Priority.HasValue);
    }
}
