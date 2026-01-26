using FamilyRelocation.Domain.Enums;

namespace FamilyRelocation.Application.Reminders.DTOs;

/// <summary>
/// DTO for follow-up reminder data.
/// </summary>
public class ReminderDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTime DueDateTime { get; init; }
    public ReminderPriority Priority { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public string? EntityDisplayName { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public string? AssignedToUserName { get; init; }
    public ReminderStatus Status { get; init; }
    public bool SendEmailNotification { get; init; }
    public DateTime? SnoozedUntil { get; init; }
    public int SnoozeCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid CreatedBy { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime? CompletedAt { get; init; }
    public Guid? CompletedBy { get; init; }
    public bool IsOverdue { get; init; }
    public bool IsDueToday { get; init; }

    // Source activity info (if reminder was created from an activity)
    public Guid? SourceActivityId { get; init; }
    public string? SourceActivityType { get; init; }
    public DateTime? SourceActivityTimestamp { get; init; }
}
