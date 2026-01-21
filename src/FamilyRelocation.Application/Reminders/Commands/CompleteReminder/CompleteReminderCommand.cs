using MediatR;

namespace FamilyRelocation.Application.Reminders.Commands.CompleteReminder;

/// <summary>
/// Command to mark a reminder as completed.
/// </summary>
public record CompleteReminderCommand(Guid ReminderId) : IRequest<Unit>;
