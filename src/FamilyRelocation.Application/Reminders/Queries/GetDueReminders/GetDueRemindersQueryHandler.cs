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

    public GetDueRemindersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DueRemindersReportDto> Handle(GetDueRemindersQuery query, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var upcomingEndDate = today.AddDays(query.UpcomingDays);

        // Include both Open and Snoozed reminders
        // For Snoozed reminders, use SnoozedUntil as the effective due date
        var baseQuery = _context.Set<FollowUpReminder>()
            .Where(r => r.Status == ReminderStatus.Open || r.Status == ReminderStatus.Snoozed);

        if (query.AssignedToUserId.HasValue)
            baseQuery = baseQuery.Where(r => r.AssignedToUserId == query.AssignedToUserId.Value);

        // For filtering, use SnoozedUntil as the effective due date for snoozed reminders
        // Use date range comparisons to handle dates with time components
        // Get overdue reminders (Open with DueDate < today OR Snoozed with SnoozedUntil < today)
        var overdueReminders = await baseQuery
            .Where(r =>
                (r.Status == ReminderStatus.Open && r.DueDate < today) ||
                (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value < today))
            .OrderBy(r => r.Status == ReminderStatus.Snoozed ? r.SnoozedUntil : r.DueDate)
            .ThenByDescending(r => r.Priority)
            .Take(20)
            .ToListAsync(cancellationToken);

        // Get due today reminders (DueDate >= today AND DueDate < tomorrow)
        var dueTodayReminders = await baseQuery
            .Where(r =>
                (r.Status == ReminderStatus.Open && r.DueDate >= today && r.DueDate < tomorrow) ||
                (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value >= today && r.SnoozedUntil.Value < tomorrow))
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.DueTime)
            .Take(20)
            .ToListAsync(cancellationToken);

        // Get upcoming reminders (next N days, excluding today)
        var upcomingReminders = await baseQuery
            .Where(r =>
                (r.Status == ReminderStatus.Open && r.DueDate >= tomorrow && r.DueDate < upcomingEndDate.AddDays(1)) ||
                (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value >= tomorrow && r.SnoozedUntil.Value < upcomingEndDate.AddDays(1)))
            .OrderBy(r => r.Status == ReminderStatus.Snoozed ? r.SnoozedUntil : r.DueDate)
            .ThenByDescending(r => r.Priority)
            .Take(20)
            .ToListAsync(cancellationToken);

        // Get counts using same range-based logic
        var overdueCount = await baseQuery.CountAsync(r =>
            (r.Status == ReminderStatus.Open && r.DueDate < today) ||
            (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value < today),
            cancellationToken);
        var dueTodayCount = await baseQuery.CountAsync(r =>
            (r.Status == ReminderStatus.Open && r.DueDate >= today && r.DueDate < tomorrow) ||
            (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value >= today && r.SnoozedUntil.Value < tomorrow),
            cancellationToken);
        var upcomingCount = await baseQuery.CountAsync(r =>
            (r.Status == ReminderStatus.Open && r.DueDate >= tomorrow && r.DueDate < upcomingEndDate.AddDays(1)) ||
            (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value >= tomorrow && r.SnoozedUntil.Value < upcomingEndDate.AddDays(1)),
            cancellationToken);
        var totalOpenCount = await baseQuery.CountAsync(cancellationToken);

        return new DueRemindersReportDto
        {
            Overdue = MapToDto(overdueReminders),
            DueToday = MapToDto(dueTodayReminders),
            Upcoming = MapToDto(upcomingReminders),
            OverdueCount = overdueCount,
            DueTodayCount = dueTodayCount,
            UpcomingCount = upcomingCount,
            TotalOpenCount = totalOpenCount
        };
    }

    private static List<ReminderDto> MapToDto(List<FollowUpReminder> reminders)
    {
        return reminders.Select(r => new ReminderDto
        {
            Id = r.Id,
            Title = r.Title,
            Notes = r.Notes,
            DueDate = r.DueDate,
            DueTime = r.DueTime,
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
            IsOverdue = r.IsOverdue,
            IsDueToday = r.IsDueToday
        }).ToList();
    }
}
