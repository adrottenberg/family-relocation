using FamilyRelocation.Domain.Enums;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Represents a follow-up reminder for any entity in the system.
/// </summary>
public class FollowUpReminder
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public DateTime DueDate { get; private set; }
    public TimeOnly? DueTime { get; private set; }
    public ReminderPriority Priority { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public ReminderStatus Status { get; private set; }
    public bool SendEmailNotification { get; private set; }
    public DateTime? SnoozedUntil { get; private set; }
    public int SnoozeCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? CompletedBy { get; private set; }

    private FollowUpReminder() { }

    /// <summary>
    /// Creates a new follow-up reminder.
    /// </summary>
    public static FollowUpReminder Create(
        string title,
        DateTime dueDate,
        string entityType,
        Guid entityId,
        Guid createdBy,
        string? notes = null,
        TimeOnly? dueTime = null,
        ReminderPriority priority = ReminderPriority.Normal,
        Guid? assignedToUserId = null,
        bool sendEmailNotification = false)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));

        if (title.Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters", nameof(title));

        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required", nameof(entityType));

        if (dueDate.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Due date cannot be in the past", nameof(dueDate));

        return new FollowUpReminder
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Notes = notes?.Trim(),
            DueDate = dueDate.Date,
            DueTime = dueTime,
            Priority = priority,
            EntityType = entityType,
            EntityId = entityId,
            AssignedToUserId = assignedToUserId,
            Status = ReminderStatus.Open,
            SendEmailNotification = sendEmailNotification,
            SnoozeCount = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Marks the reminder as completed.
    /// </summary>
    public void Complete(Guid userId)
    {
        if (Status == ReminderStatus.Completed)
            throw new InvalidOperationException("Reminder is already completed");

        Status = ReminderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        CompletedBy = userId;
        SnoozedUntil = null;
    }

    /// <summary>
    /// Snoozes the reminder until the specified date.
    /// </summary>
    public void Snooze(DateTime snoozeUntil, Guid userId)
    {
        if (Status == ReminderStatus.Completed)
            throw new InvalidOperationException("Cannot snooze a completed reminder");

        if (snoozeUntil.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Snooze date cannot be in the past", nameof(snoozeUntil));

        Status = ReminderStatus.Snoozed;
        SnoozedUntil = snoozeUntil.Date;
        SnoozeCount++;
    }

    /// <summary>
    /// Dismisses the reminder (soft delete).
    /// </summary>
    public void Dismiss(Guid userId)
    {
        if (Status == ReminderStatus.Completed)
            throw new InvalidOperationException("Cannot dismiss a completed reminder");

        Status = ReminderStatus.Dismissed;
        CompletedAt = DateTime.UtcNow;
        CompletedBy = userId;
    }

    /// <summary>
    /// Reopens a completed or dismissed reminder.
    /// </summary>
    public void Reopen()
    {
        if (Status == ReminderStatus.Open || Status == ReminderStatus.Snoozed)
            throw new InvalidOperationException("Reminder is already open");

        Status = ReminderStatus.Open;
        CompletedAt = null;
        CompletedBy = null;
        SnoozedUntil = null;
    }

    /// <summary>
    /// Updates the reminder details.
    /// </summary>
    public void Update(
        string? title = null,
        DateTime? dueDate = null,
        TimeOnly? dueTime = null,
        ReminderPriority? priority = null,
        string? notes = null,
        Guid? assignedToUserId = null,
        bool? sendEmailNotification = null)
    {
        if (title != null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty", nameof(title));
            if (title.Length > 200)
                throw new ArgumentException("Title cannot exceed 200 characters", nameof(title));
            Title = title.Trim();
        }

        if (dueDate.HasValue)
        {
            if (dueDate.Value.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("Due date cannot be in the past", nameof(dueDate));
            DueDate = dueDate.Value.Date;
        }

        if (dueTime.HasValue)
            DueTime = dueTime;

        if (priority.HasValue)
            Priority = priority.Value;

        if (notes != null)
            Notes = notes.Trim();

        if (assignedToUserId.HasValue)
            AssignedToUserId = assignedToUserId;

        if (sendEmailNotification.HasValue)
            SendEmailNotification = sendEmailNotification.Value;
    }

    /// <summary>
    /// Checks if the reminder is overdue.
    /// </summary>
    public bool IsOverdue => Status == ReminderStatus.Open &&
        (DueDate < DateTime.UtcNow.Date ||
         (DueDate == DateTime.UtcNow.Date && DueTime.HasValue && DueTime.Value < TimeOnly.FromDateTime(DateTime.UtcNow)));

    /// <summary>
    /// Checks if the reminder is due today.
    /// </summary>
    public bool IsDueToday => Status == ReminderStatus.Open && DueDate == DateTime.UtcNow.Date;

    /// <summary>
    /// Gets the effective due date (considering snooze).
    /// </summary>
    public DateTime EffectiveDueDate => SnoozedUntil ?? DueDate;
}
