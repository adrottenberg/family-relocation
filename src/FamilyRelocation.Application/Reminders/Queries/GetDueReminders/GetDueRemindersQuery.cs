using FamilyRelocation.Application.Reminders.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Reminders.Queries.GetDueReminders;

/// <summary>
/// Query to get reminders that are due (overdue + due today + upcoming).
/// This is optimized for the dashboard widget.
/// </summary>
public record GetDueRemindersQuery(
    int UpcomingDays = 7,
    Guid? AssignedToUserId = null) : IRequest<DueRemindersReportDto>;
