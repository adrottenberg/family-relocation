using FamilyRelocation.Domain.Common;
using FamilyRelocation.Domain.Enums;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Represents a property showing appointment.
/// A showing is always linked to a PropertyMatch.
/// </summary>
public class Showing : Entity<Guid>
{
    public Guid PropertyMatchId { get; private set; }
    public DateOnly ScheduledDate { get; private set; }
    public TimeOnly ScheduledTime { get; private set; }
    public ShowingStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public Guid? BrokerUserId { get; private set; }

    // Navigation property
    public virtual PropertyMatch PropertyMatch { get; private set; } = null!;

    // Audit fields
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? ModifiedBy { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private Showing() { }

    /// <summary>
    /// Factory method to create a new showing.
    /// </summary>
    public static Showing Create(
        Guid propertyMatchId,
        DateOnly scheduledDate,
        TimeOnly scheduledTime,
        Guid createdBy,
        string? notes = null,
        Guid? brokerUserId = null)
    {
        if (propertyMatchId == Guid.Empty)
            throw new ArgumentException("Property match ID is required", nameof(propertyMatchId));

        // Validate scheduled date is not in the past
        var scheduledDateTime = scheduledDate.ToDateTime(scheduledTime);
        if (scheduledDateTime < DateTime.UtcNow.AddHours(-1)) // Allow some flexibility
            throw new ArgumentException("Cannot schedule a showing in the past", nameof(scheduledDate));

        return new Showing
        {
            Id = Guid.NewGuid(),
            PropertyMatchId = propertyMatchId,
            ScheduledDate = scheduledDate,
            ScheduledTime = scheduledTime,
            Status = ShowingStatus.Scheduled,
            Notes = notes,
            BrokerUserId = brokerUserId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Reschedules the showing to a new date/time.
    /// </summary>
    public void Reschedule(DateOnly newDate, TimeOnly newTime, Guid modifiedBy)
    {
        if (Status != ShowingStatus.Scheduled)
            throw new InvalidOperationException("Can only reschedule a scheduled showing");

        var scheduledDateTime = newDate.ToDateTime(newTime);
        if (scheduledDateTime < DateTime.UtcNow.AddHours(-1))
            throw new ArgumentException("Cannot reschedule to a past date/time", nameof(newDate));

        ScheduledDate = newDate;
        ScheduledTime = newTime;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the showing as completed.
    /// </summary>
    public void MarkCompleted(Guid modifiedBy, string? notes = null)
    {
        if (Status != ShowingStatus.Scheduled)
            throw new InvalidOperationException("Can only complete a scheduled showing");

        Status = ShowingStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        if (notes != null)
        {
            Notes = string.IsNullOrEmpty(Notes) ? notes : $"{Notes}\n\n{notes}";
        }
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the showing.
    /// </summary>
    public void Cancel(Guid modifiedBy, string? reason = null)
    {
        if (Status != ShowingStatus.Scheduled)
            throw new InvalidOperationException("Can only cancel a scheduled showing");

        Status = ShowingStatus.Cancelled;
        if (reason != null)
        {
            Notes = string.IsNullOrEmpty(Notes) ? $"Cancelled: {reason}" : $"{Notes}\n\nCancelled: {reason}";
        }
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the showing as a no-show.
    /// </summary>
    public void MarkNoShow(Guid modifiedBy, string? notes = null)
    {
        if (Status != ShowingStatus.Scheduled)
            throw new InvalidOperationException("Can only mark a scheduled showing as no-show");

        Status = ShowingStatus.NoShow;
        if (notes != null)
        {
            Notes = string.IsNullOrEmpty(Notes) ? $"No-show: {notes}" : $"{Notes}\n\nNo-show: {notes}";
        }
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the assigned broker.
    /// </summary>
    public void AssignBroker(Guid? brokerUserId, Guid modifiedBy)
    {
        BrokerUserId = brokerUserId;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the notes for this showing.
    /// </summary>
    public void UpdateNotes(string? notes, Guid modifiedBy)
    {
        Notes = notes;
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the scheduled date/time as a DateTime.
    /// </summary>
    public DateTime ScheduledDateTime => ScheduledDate.ToDateTime(ScheduledTime);

    /// <summary>
    /// Indicates if the showing is upcoming (scheduled and in the future).
    /// </summary>
    public bool IsUpcoming => Status == ShowingStatus.Scheduled && ScheduledDateTime > DateTime.UtcNow;
}
