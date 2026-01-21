using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Reminders.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Reminders.Commands.UpdateReminder;

/// <summary>
/// Handler for UpdateReminderCommand.
/// </summary>
public class UpdateReminderCommandHandler : IRequestHandler<UpdateReminderCommand, ReminderDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateReminderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReminderDto> Handle(UpdateReminderCommand command, CancellationToken cancellationToken)
    {
        var reminder = await _context.Set<FollowUpReminder>()
            .FirstOrDefaultAsync(r => r.Id == command.ReminderId, cancellationToken)
            ?? throw new NotFoundException("Reminder", command.ReminderId);

        reminder.Update(
            command.Title,
            command.DueDate,
            command.DueTime,
            command.Priority,
            command.Notes,
            command.AssignedToUserId,
            command.SendEmailNotification);

        await _context.SaveChangesAsync(cancellationToken);

        return new ReminderDto
        {
            Id = reminder.Id,
            Title = reminder.Title,
            Notes = reminder.Notes,
            DueDate = reminder.DueDate,
            DueTime = reminder.DueTime,
            Priority = reminder.Priority,
            EntityType = reminder.EntityType,
            EntityId = reminder.EntityId,
            AssignedToUserId = reminder.AssignedToUserId,
            Status = reminder.Status,
            SendEmailNotification = reminder.SendEmailNotification,
            SnoozedUntil = reminder.SnoozedUntil,
            SnoozeCount = reminder.SnoozeCount,
            CreatedAt = reminder.CreatedAt,
            CreatedBy = reminder.CreatedBy,
            CompletedAt = reminder.CompletedAt,
            CompletedBy = reminder.CompletedBy,
            IsOverdue = reminder.IsOverdue,
            IsDueToday = reminder.IsDueToday
        };
    }
}
