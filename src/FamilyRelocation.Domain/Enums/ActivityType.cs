namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Types of activities/interactions
/// Updated: Expanded per Correction #1
/// </summary>
public enum ActivityType
{
    Note,
    EmailSent,
    EmailReceived,
    StageChange,
    StatusChange,

    // Added per Correction #1
    PhoneCall,
    TextMessage,
    Meeting,
    ShowingScheduled,
    ShowingCompleted,
    DocumentUploaded,
    DocumentSigned,
    ReminderCreated,
    ReminderCompleted
}
