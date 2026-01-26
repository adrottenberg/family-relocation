using FamilyRelocation.Application.Reminders.DTOs;
using FamilyRelocation.Domain.Enums;
using MediatR;

namespace FamilyRelocation.Application.Reminders.Queries.GetReminders;

/// <summary>
/// Query to get reminders with optional filtering.
/// </summary>
public record GetRemindersQuery(
    string? EntityType = null,
    Guid? EntityId = null,
    ReminderStatus? Status = null,
    ReminderPriority? Priority = null,
    Guid? AssignedToUserId = null,
    DateTime? DueDateTimeFrom = null,
    DateTime? DueDateTimeTo = null,
    bool? IncludeOverdueOnly = null,
    bool? IncludeDueTodayOnly = null,
    int Skip = 0,
    int Take = 50) : IRequest<RemindersListDto>;
