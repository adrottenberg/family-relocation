using MediatR;

namespace FamilyRelocation.Application.Reminders.Commands.SnoozeReminder;

/// <summary>
/// Command to snooze a reminder until a specified date.
/// </summary>
public record SnoozeReminderCommand(Guid ReminderId, DateTime SnoozeUntil) : IRequest<Unit>;
