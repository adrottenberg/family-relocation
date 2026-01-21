namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Status of a follow-up reminder.
/// </summary>
public enum ReminderStatus
{
    Open = 0,
    Completed = 1,
    Snoozed = 2,
    Dismissed = 3
}
