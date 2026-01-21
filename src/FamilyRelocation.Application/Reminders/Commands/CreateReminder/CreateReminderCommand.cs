using FamilyRelocation.Application.Reminders.DTOs;
using FamilyRelocation.Domain.Enums;
using MediatR;

namespace FamilyRelocation.Application.Reminders.Commands.CreateReminder;

/// <summary>
/// Command to create a new follow-up reminder.
/// </summary>
public record CreateReminderCommand(
    string Title,
    DateTime DueDate,
    string EntityType,
    Guid EntityId,
    string? Notes = null,
    TimeOnly? DueTime = null,
    ReminderPriority Priority = ReminderPriority.Normal,
    Guid? AssignedToUserId = null,
    bool SendEmailNotification = false) : IRequest<ReminderDto>;
