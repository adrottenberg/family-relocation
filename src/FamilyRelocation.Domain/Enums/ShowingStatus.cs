namespace FamilyRelocation.Domain.Enums;

/// <summary>
/// Status of a property showing.
/// </summary>
public enum ShowingStatus
{
    /// <summary>Showing is scheduled for a future date</summary>
    Scheduled = 0,

    /// <summary>Showing has been completed</summary>
    Completed = 1,

    /// <summary>Showing was cancelled</summary>
    Cancelled = 2,

    /// <summary>Scheduled showing was missed (no-show)</summary>
    NoShow = 3
}
