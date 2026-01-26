using FluentValidation;

namespace FamilyRelocation.Application.Reminders.Commands.CreateReminder;

/// <summary>
/// Validator for CreateReminderCommand.
/// </summary>
public class CreateReminderCommandValidator : AbstractValidator<CreateReminderCommand>
{
    public CreateReminderCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.DueDateTime)
            .NotEmpty().WithMessage("Due date/time is required.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-5)).WithMessage("Due date/time cannot be in the past.");

        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("Entity type is required.")
            .MaximumLength(50).WithMessage("Entity type cannot exceed 50 characters.");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("Entity ID is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.")
            .When(x => x.Notes != null);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid priority value.");
    }
}
