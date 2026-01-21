using MediatR;

namespace FamilyRelocation.Application.Reminders.Commands.ReopenReminder;

/// <summary>
/// Command to reopen a completed or dismissed reminder.
/// </summary>
public record ReopenReminderCommand(Guid ReminderId) : IRequest<Unit>;
