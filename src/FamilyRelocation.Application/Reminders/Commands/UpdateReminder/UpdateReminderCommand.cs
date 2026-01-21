using FamilyRelocation.Application.Reminders.DTOs;
using FamilyRelocation.Domain.Enums;
using MediatR;

namespace FamilyRelocation.Application.Reminders.Commands.UpdateReminder;

/// <summary>
/// Command to update an existing reminder.
/// </summary>
public record UpdateReminderCommand(
    Guid ReminderId,
    string? Title = null,
    DateTime? DueDate = null,
    TimeOnly? DueTime = null,
    ReminderPriority? Priority = null,
    string? Notes = null,
    Guid? AssignedToUserId = null,
    bool? SendEmailNotification = null) : IRequest<ReminderDto>;
