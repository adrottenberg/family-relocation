using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Reminders.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Reminders.Queries.GetReminders;

/// <summary>
/// Handler for GetRemindersQuery.
/// </summary>
public class GetRemindersQueryHandler : IRequestHandler<GetRemindersQuery, RemindersListDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserTimezoneService _timezoneService;

    public GetRemindersQueryHandler(
        IApplicationDbContext context,
        IUserTimezoneService timezoneService)
    {
        _context = context;
        _timezoneService = timezoneService;
    }

    public async Task<RemindersListDto> Handle(GetRemindersQuery query, CancellationToken cancellationToken)
    {
        // Normalize pagination (max 100 per request)
        var skip = Math.Max(0, query.Skip);
        var take = Math.Clamp(query.Take, 1, 100);

        // Get timezone-aware day boundaries
        var todayStart = await _timezoneService.GetTodayStartUtcAsync();
        var todayEnd = await _timezoneService.GetTodayEndUtcAsync();
        var now = DateTime.UtcNow;

        var queryable = _context.Set<FollowUpReminder>().AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(query.EntityType))
            queryable = queryable.Where(r => r.EntityType == query.EntityType);

        if (query.EntityId.HasValue)
            queryable = queryable.Where(r => r.EntityId == query.EntityId.Value);

        if (query.Status.HasValue)
            queryable = queryable.Where(r => r.Status == query.Status.Value);

        if (query.Priority.HasValue)
            queryable = queryable.Where(r => r.Priority == query.Priority.Value);

        if (query.AssignedToUserId.HasValue)
            queryable = queryable.Where(r => r.AssignedToUserId == query.AssignedToUserId.Value);

        if (query.DueDateTimeFrom.HasValue)
            queryable = queryable.Where(r => r.DueDateTime >= query.DueDateTimeFrom.Value);

        if (query.DueDateTimeTo.HasValue)
            queryable = queryable.Where(r => r.DueDateTime <= query.DueDateTimeTo.Value);

        // Include snoozed reminders - use SnoozedUntil as effective due date for snoozed reminders
        if (query.IncludeOverdueOnly == true)
            queryable = queryable.Where(r =>
                (r.Status == ReminderStatus.Open && r.DueDateTime < now) ||
                (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value < now));

        if (query.IncludeDueTodayOnly == true)
            queryable = queryable.Where(r =>
                (r.Status == ReminderStatus.Open && r.DueDateTime >= todayStart && r.DueDateTime <= todayEnd) ||
                (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value >= todayStart && r.SnoozedUntil.Value <= todayEnd));

        // Get counts - snoozed reminders only count as overdue once their snooze expires
        var overdueCount = await _context.Set<FollowUpReminder>()
            .Where(r =>
                (r.Status == ReminderStatus.Open && r.DueDateTime < now) ||
                (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil.HasValue && r.SnoozedUntil.Value < now))
            .CountAsync(cancellationToken);

        // Due today only includes Open reminders (snoozed don't count until snooze expires)
        var dueTodayCount = await _context.Set<FollowUpReminder>()
            .Where(r => r.Status == ReminderStatus.Open && r.DueDateTime >= now && r.DueDateTime <= todayEnd)
            .CountAsync(cancellationToken);

        var totalCount = await queryable.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var reminders = await queryable
            .OrderBy(r => r.DueDateTime)
            .ThenByDescending(r => r.Priority)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        // Fetch applicant names for reminders linked to applicants
        var applicantIds = reminders
            .Where(r => r.EntityType == "Applicant")
            .Select(r => r.EntityId)
            .Distinct()
            .ToList();

        var applicantNames = new Dictionary<Guid, string>();
        if (applicantIds.Count > 0)
        {
            applicantNames = await _context.Set<Applicant>()
                .Where(a => applicantIds.Contains(a.Id))
                .Select(a => new { a.Id, a.Husband.FirstName, a.Husband.LastName })
                .ToDictionaryAsync(
                    a => a.Id,
                    a => $"{a.FirstName} {a.LastName}",
                    cancellationToken);
        }

        // Compute IsOverdue and IsDueToday for each reminder
        // - Snoozed reminders only become overdue once their snooze expires (SnoozedUntil < now)
        // - Snoozed reminders are never "due today" - they're either still snoozed or overdue
        var items = new List<ReminderDto>();
        foreach (var r in reminders)
        {
            var effectiveDue = r.EffectiveDueDateTime;
            bool isOverdue;
            bool isDueToday;

            if (r.Status == ReminderStatus.Snoozed)
            {
                // Snoozed: only overdue if snooze has expired
                isOverdue = r.SnoozedUntil.HasValue && r.SnoozedUntil.Value < now;
                isDueToday = false; // Snoozed reminders are never "due today"
            }
            else if (r.Status == ReminderStatus.Open)
            {
                isOverdue = r.DueDateTime < now;
                isDueToday = r.DueDateTime >= now && r.DueDateTime <= todayEnd;
            }
            else
            {
                // Completed or Dismissed
                isOverdue = false;
                isDueToday = false;
            }

            items.Add(new ReminderDto
            {
                Id = r.Id,
                Title = r.Title,
                Notes = r.Notes,
                DueDateTime = r.DueDateTime,
                Priority = r.Priority,
                EntityType = r.EntityType,
                EntityId = r.EntityId,
                EntityDisplayName = r.EntityType == "Applicant" && applicantNames.TryGetValue(r.EntityId, out var name) ? name : null,
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
            });
        }

        return new RemindersListDto
        {
            Items = items,
            TotalCount = totalCount,
            OverdueCount = overdueCount,
            DueTodayCount = dueTodayCount
        };
    }
}
