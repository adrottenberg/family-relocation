using FamilyRelocation.Domain.Enums;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Represents an activity log entry for tracking system events and manual communications.
/// </summary>
public class ActivityLog
{
    public Guid Id { get; private set; }
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public Guid? UserId { get; private set; }
    public string? UserName { get; private set; }
    public DateTime Timestamp { get; private set; }

    // New fields for communication logging
    public ActivityType Type { get; private set; } = ActivityType.System;
    public int? DurationMinutes { get; private set; }
    public string? Outcome { get; private set; }
    public Guid? FollowUpReminderId { get; private set; }

    private ActivityLog() { }

    /// <summary>
    /// Creates a system-generated activity log entry (for automatic logging).
    /// </summary>
    public static ActivityLog Create(
        string entityType,
        Guid entityId,
        string action,
        string description,
        Guid? userId = null,
        string? userName = null)
    {
        return new ActivityLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType)),
            EntityId = entityId,
            Action = action ?? throw new ArgumentNullException(nameof(action)),
            Description = description ?? throw new ArgumentNullException(nameof(description)),
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.UtcNow,
            Type = ActivityType.System
        };
    }

    /// <summary>
    /// Creates a phone call activity log entry.
    /// </summary>
    public static ActivityLog CreatePhoneCall(
        string entityType,
        Guid entityId,
        string description,
        int? durationMinutes,
        string? outcome,
        Guid? userId = null,
        string? userName = null,
        Guid? followUpReminderId = null)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required", nameof(entityType));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        return new ActivityLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = "PhoneCall",
            Description = description.Trim(),
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.UtcNow,
            Type = ActivityType.PhoneCall,
            DurationMinutes = durationMinutes,
            Outcome = outcome?.Trim(),
            FollowUpReminderId = followUpReminderId
        };
    }

    /// <summary>
    /// Creates a note activity log entry.
    /// </summary>
    public static ActivityLog CreateNote(
        string entityType,
        Guid entityId,
        string description,
        Guid? userId = null,
        string? userName = null,
        Guid? followUpReminderId = null)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required", nameof(entityType));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        return new ActivityLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = "Note",
            Description = description.Trim(),
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.UtcNow,
            Type = ActivityType.Note,
            FollowUpReminderId = followUpReminderId
        };
    }

    /// <summary>
    /// Creates an email activity log entry.
    /// </summary>
    public static ActivityLog CreateEmail(
        string entityType,
        Guid entityId,
        string description,
        Guid? userId = null,
        string? userName = null,
        Guid? followUpReminderId = null)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required", nameof(entityType));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        return new ActivityLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = "Email",
            Description = description.Trim(),
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.UtcNow,
            Type = ActivityType.Email,
            FollowUpReminderId = followUpReminderId
        };
    }

    /// <summary>
    /// Creates an SMS activity log entry.
    /// </summary>
    public static ActivityLog CreateSMS(
        string entityType,
        Guid entityId,
        string description,
        Guid? userId = null,
        string? userName = null,
        Guid? followUpReminderId = null)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required", nameof(entityType));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        return new ActivityLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = "SMS",
            Description = description.Trim(),
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.UtcNow,
            Type = ActivityType.SMS,
            FollowUpReminderId = followUpReminderId
        };
    }
}
