using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Reminders.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Reminders.Queries.GetDueReminders;

/// <summary>
/// Handler for GetDueRemindersQuery.
/// </summary>
public class GetDueRemindersQueryHandler : IRequestHandler<GetDueRemindersQuery, DueRemindersReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserTimezoneService _timezoneService;

    public GetDueRemindersQueryHandler(
        IApplicationDbContext context,
        IUserTimezoneService timezoneService)
    {
        _context = context;
        _timezoneService = timezoneService;
    }

    public async Task<DueRemindersReportDto> Handle(GetDueRemindersQuery query, CancellationToken cancellationToken)
    {
        // Get timezone-aware day boundaries
        var todayStart = await _timezoneService.GetTodayStartUtcAsync();
        var todayEnd = await _timezoneService.GetTodayEndUtcAsync();
        var tomorrowStart = todayEnd.AddTicks(1);
        var upcomingEndDate = todayStart.AddDays(query.UpcomingDays + 1);
        var now = DateTime.UtcNow;

        // Include both Open and Snoozed reminders
        // For Snoozed reminders, use SnoozedUntil as the effective due date
        var baseQuery = _context.Set<FollowUpReminder>()
            .Where(r => r.Status == ReminderStatus.Open || r.Status == ReminderStatus.Snoozed);

        if (query.AssignedToUserId.HasValue)
            baseQuery = baseQuery.Where(r => r.AssignedToUserId == query.AssignedToUserId.Value);

        // Get overdue reminders (Open with DueDateTime < now OR Snoozed with SnoozedUntil < now)
        var overdueReminders = await baseQuery
            .Where(r =>
                (r.Status == ReminderStatus.Open && r.DueDateTime < now) ||
                (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value < now))
            .OrderBy(r => r.Status == ReminderStatus.Snoozed ? r.SnoozedUntil : r.DueDateTime)
            .ThenByDescending(r => r.Priority)
            .Take(20)
            .ToListAsync(cancellationToken);

        // Get due today reminders (DueDateTime >= todayStart AND DueDateTime <= todayEnd)
        var dueTodayReminders = await baseQuery
            .Where(r =>
                (r.Status == ReminderStatus.Open && r.DueDateTime >= todayStart && r.DueDateTime <= todayEnd) ||
                (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value >= todayStart && r.SnoozedUntil.Value <= todayEnd))
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.DueDateTime)
            .Take(20)
            .ToListAsync(cancellationToken);

        // Get upcoming reminders (next N days, excluding today)
        var upcomingReminders = await baseQuery
            .Where(r =>
                (r.Status == ReminderStatus.Open && r.DueDateTime >= tomorrowStart && r.DueDateTime < upcomingEndDate) ||
                (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value >= tomorrowStart && r.SnoozedUntil.Value < upcomingEndDate))
            .OrderBy(r => r.Status == ReminderStatus.Snoozed ? r.SnoozedUntil : r.DueDateTime)
            .ThenByDescending(r => r.Priority)
            .Take(20)
            .ToListAsync(cancellationToken);

        // Get counts using same range-based logic
        var overdueCount = await baseQuery.CountAsync(r =>
            (r.Status == ReminderStatus.Open && r.DueDateTime < now) ||
            (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value < now),
            cancellationToken);
        var dueTodayCount = await baseQuery.CountAsync(r =>
            (r.Status == ReminderStatus.Open && r.DueDateTime >= todayStart && r.DueDateTime <= todayEnd) ||
            (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value >= todayStart && r.SnoozedUntil.Value <= todayEnd),
            cancellationToken);
        var upcomingCount = await baseQuery.CountAsync(r =>
            (r.Status == ReminderStatus.Open && r.DueDateTime >= tomorrowStart && r.DueDateTime < upcomingEndDate) ||
            (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value >= tomorrowStart && r.SnoozedUntil.Value < upcomingEndDate),
            cancellationToken);
        var totalOpenCount = await baseQuery.CountAsync(cancellationToken);

        return new DueRemindersReportDto
        {
            Overdue = MapToDto(overdueReminders, now, todayStart, todayEnd),
            DueToday = MapToDto(dueTodayReminders, now, todayStart, todayEnd),
            Upcoming = MapToDto(upcomingReminders, now, todayStart, todayEnd),
            OverdueCount = overdueCount,
            DueTodayCount = dueTodayCount,
            UpcomingCount = upcomingCount,
            TotalOpenCount = totalOpenCount
        };
    }

    private static List<ReminderDto> MapToDto(List<FollowUpReminder> reminders, DateTime now, DateTime todayStart, DateTime todayEnd)
    {
        return reminders.Select(r =>
        {
            var effectiveDue = r.EffectiveDueDateTime;
            var isOverdue = effectiveDue < now && r.Status != ReminderStatus.Completed && r.Status != ReminderStatus.Dismissed;
            var isDueToday = effectiveDue >= todayStart && effectiveDue <= todayEnd && r.Status != ReminderStatus.Completed && r.Status != ReminderStatus.Dismissed;

            return new ReminderDto
            {
                Id = r.Id,
                Title = r.Title,
                Notes = r.Notes,
                DueDateTime = r.DueDateTime,
                Priority = r.Priority,
                EntityType = r.EntityType,
                EntityId = r.EntityId,
                AssignedToUserId = r.AssignedToUserId,
                Status = r.Status,
                SendEmailNotification = r.SendEmailNotification,
                SnoozedUntil = r.SnoozedUntil,
                SnoozeCount = r.SnoozeCount,
                CreatedAt = r.CreatedAt,
                CreatedBy = r.CreatedBy,
                CompletedAt = r.CompletedAt,
                CompletedBy = r.CompletedBy,
                IsOverdue = isOverdue,
                IsDueToday = isDueToday
            };
        }).ToList();
    }
}
