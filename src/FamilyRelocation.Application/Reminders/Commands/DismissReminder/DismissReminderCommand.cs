using MediatR;

namespace FamilyRelocation.Application.Reminders.Commands.DismissReminder;

/// <summary>
/// Command to dismiss a reminder (soft delete).
/// </summary>
public record DismissReminderCommand(Guid ReminderId) : IRequest<Unit>;
