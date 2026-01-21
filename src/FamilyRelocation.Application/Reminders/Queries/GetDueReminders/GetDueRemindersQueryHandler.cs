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
        var upcomingEndDate = today.AddDays(query.UpcomingDays);

        var baseQuery = _context.Set<FollowUpReminder>()
            .Where(r => r.Status == ReminderStatus.Open);

        if (query.AssignedToUserId.HasValue)
            baseQuery = baseQuery.Where(r => r.AssignedToUserId == query.AssignedToUserId.Value);

        // Get overdue reminders
        var overdueReminders = await baseQuery
            .Where(r => r.DueDate < today)
            .OrderBy(r => r.DueDate)
            .ThenByDescending(r => r.Priority)
            .Take(20)
            .ToListAsync(cancellationToken);

        // Get due today reminders
        var dueTodayReminders = await baseQuery
            .Where(r => r.DueDate == today)
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.DueTime)
            .Take(20)
            .ToListAsync(cancellationToken);

        // Get upcoming reminders (next N days, excluding today)
        var upcomingReminders = await baseQuery
            .Where(r => r.DueDate > today && r.DueDate <= upcomingEndDate)
            .OrderBy(r => r.DueDate)
            .ThenByDescending(r => r.Priority)
            .Take(20)
            .ToListAsync(cancellationToken);

        // Get counts
        var overdueCount = await baseQuery.CountAsync(r => r.DueDate < today, cancellationToken);
        var dueTodayCount = await baseQuery.CountAsync(r => r.DueDate == today, cancellationToken);
        var upcomingCount = await baseQuery.CountAsync(r => r.DueDate > today && r.DueDate <= upcomingEndDate, cancellationToken);
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
