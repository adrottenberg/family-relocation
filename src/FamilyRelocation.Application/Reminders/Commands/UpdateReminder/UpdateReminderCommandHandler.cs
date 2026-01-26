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
    private readonly IUserTimezoneService _timezoneService;

    public UpdateReminderCommandHandler(
        IApplicationDbContext context,
        IUserTimezoneService timezoneService)
    {
        _context = context;
        _timezoneService = timezoneService;
    }

    public async Task<ReminderDto> Handle(UpdateReminderCommand command, CancellationToken cancellationToken)
    {
        var reminder = await _context.Set<FollowUpReminder>()
            .FirstOrDefaultAsync(r => r.Id == command.ReminderId, cancellationToken)
            ?? throw new NotFoundException("Reminder", command.ReminderId);

        reminder.Update(
            command.Title,
            command.DueDateTime,
            command.Priority,
            command.Notes,
            command.AssignedToUserId,
            command.SendEmailNotification);

        await _context.SaveChangesAsync(cancellationToken);

        // Compute timezone-aware IsOverdue and IsDueToday
        var isOverdue = await _timezoneService.IsOverdueAsync(reminder.EffectiveDueDateTime);
        var isDueToday = await _timezoneService.IsTodayAsync(reminder.EffectiveDueDateTime);

        return new ReminderDto
        {
            Id = reminder.Id,
            Title = reminder.Title,
            Notes = reminder.Notes,
            DueDateTime = reminder.DueDateTime,
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
            IsOverdue = isOverdue,
            IsDueToday = isDueToday
        };
    }
}
