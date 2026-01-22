namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Types of activity log entries.
/// </summary>
public enum ActivityType
{
    /// <summary>
    /// System-generated activity (automatic logging).
    /// </summary>
    System = 0,

    /// <summary>
    /// Phone call communication.
    /// </summary>
    PhoneCall = 1,

    /// <summary>
    /// Email communication.
    /// </summary>
    Email = 2,

    /// <summary>
    /// SMS/Text message communication.
    /// </summary>
    SMS = 3,

    /// <summary>
    /// General note or comment.
    /// </summary>
    Note = 4
}
