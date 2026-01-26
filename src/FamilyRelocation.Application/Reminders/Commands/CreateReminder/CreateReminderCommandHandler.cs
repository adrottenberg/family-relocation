using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Reminders.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;

namespace FamilyRelocation.Application.Reminders.Commands.CreateReminder;

/// <summary>
/// Handler for CreateReminderCommand.
/// </summary>
public class CreateReminderCommandHandler : IRequestHandler<CreateReminderCommand, ReminderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserTimezoneService _timezoneService;

    public CreateReminderCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUserTimezoneService timezoneService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _timezoneService = timezoneService;
    }

    public async Task<ReminderDto> Handle(CreateReminderCommand command, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to create reminders.");

        var reminder = FollowUpReminder.Create(
            command.Title,
            command.DueDateTime,
            command.EntityType,
            command.EntityId,
            userId,
            command.Notes,
            command.Priority,
            command.AssignedToUserId,
            command.SendEmailNotification);

        _context.Add(reminder);
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
