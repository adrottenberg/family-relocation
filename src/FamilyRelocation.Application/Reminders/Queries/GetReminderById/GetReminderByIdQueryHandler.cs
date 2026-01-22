using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Reminders.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Reminders.Queries.GetReminderById;

/// <summary>
/// Handler for GetReminderByIdQuery.
/// </summary>
public class GetReminderByIdQueryHandler : IRequestHandler<GetReminderByIdQuery, ReminderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetReminderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReminderDto?> Handle(GetReminderByIdQuery query, CancellationToken cancellationToken)
    {
        var reminder = await _context.Set<FollowUpReminder>()
            .FirstOrDefaultAsync(r => r.Id == query.ReminderId, cancellationToken);

        if (reminder == null)
            return null;

        // Get entity display name
        string? entityDisplayName = null;
        if (reminder.EntityType == "Applicant")
        {
            var applicant = await _context.Set<Applicant>()
                .Where(a => a.Id == reminder.EntityId && !a.IsDeleted)
                .Select(a => new { a.Husband.FirstName, a.Husband.LastName })
                .FirstOrDefaultAsync(cancellationToken);

            if (applicant != null)
            {
                entityDisplayName = $"{applicant.FirstName} {applicant.LastName}";
            }
        }

        // Find source activity (if this reminder was created from an activity)
        var sourceActivity = await _context.Set<ActivityLog>()
            .Where(a => a.FollowUpReminderId == reminder.Id)
            .Select(a => new { a.Id, a.Type, a.Timestamp })
            .FirstOrDefaultAsync(cancellationToken);

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
            EntityDisplayName = entityDisplayName,
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
            IsDueToday = reminder.IsDueToday,
            SourceActivityId = sourceActivity?.Id,
            SourceActivityType = sourceActivity?.Type.ToString(),
            SourceActivityTimestamp = sourceActivity?.Timestamp
        };
    }
}
