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

    public GetRemindersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RemindersListDto> Handle(GetRemindersQuery query, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

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

        if (query.DueDateFrom.HasValue)
            queryable = queryable.Where(r => r.DueDate >= query.DueDateFrom.Value.Date);

        if (query.DueDateTo.HasValue)
            queryable = queryable.Where(r => r.DueDate <= query.DueDateTo.Value.Date);

        if (query.IncludeOverdueOnly == true)
            queryable = queryable.Where(r => r.Status == ReminderStatus.Open && r.DueDate < today);

        if (query.IncludeDueTodayOnly == true)
            queryable = queryable.Where(r => r.Status == ReminderStatus.Open && r.DueDate == today);

        // Get counts for open reminders
        var overdueCount = await _context.Set<FollowUpReminder>()
            .Where(r => r.Status == ReminderStatus.Open && r.DueDate < today)
            .CountAsync(cancellationToken);

        var dueTodayCount = await _context.Set<FollowUpReminder>()
            .Where(r => r.Status == ReminderStatus.Open && r.DueDate == today)
            .CountAsync(cancellationToken);

        var totalCount = await queryable.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var reminders = await queryable
            .OrderBy(r => r.DueDate)
            .ThenByDescending(r => r.Priority)
            .ThenBy(r => r.DueTime)
            .Skip(query.Skip)
            .Take(query.Take)
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

        var items = reminders.Select(r => new ReminderDto
        {
            Id = r.Id,
            Title = r.Title,
            Notes = r.Notes,
            DueDate = r.DueDate,
            DueTime = r.DueTime,
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
            IsOverdue = r.IsOverdue,
            IsDueToday = r.IsDueToday
        }).ToList();

        return new RemindersListDto
        {
            Items = items,
            TotalCount = totalCount,
            OverdueCount = overdueCount,
            DueTodayCount = dueTodayCount
        };
    }
}
