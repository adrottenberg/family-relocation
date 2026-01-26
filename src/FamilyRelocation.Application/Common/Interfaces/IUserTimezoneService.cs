namespace FamilyRelocation.Application.Common.Interfaces;

/// <summary>
/// Service for handling user timezone conversions.
/// </summary>
public interface IUserTimezoneService
{
    /// <summary>
    /// Gets the current user's timezone ID (e.g., "America/New_York").
    /// Defaults to "America/New_York" if not set.
    /// </summary>
    Task<string> GetCurrentUserTimezoneIdAsync();

    /// <summary>
    /// Gets the current user's timezone info.
    /// </summary>
    Task<TimeZoneInfo> GetCurrentUserTimezoneAsync();

    /// <summary>
    /// Gets the start of today in UTC based on the user's timezone.
    /// For example, if user is in ET and it's 3am ET, this returns midnight ET in UTC.
    /// </summary>
    Task<DateTime> GetTodayStartUtcAsync();

    /// <summary>
    /// Gets the end of today in UTC based on the user's timezone.
    /// Returns 23:59:59.999 in user's timezone, converted to UTC.
    /// </summary>
    Task<DateTime> GetTodayEndUtcAsync();

    /// <summary>
    /// Converts a UTC datetime to the user's local timezone.
    /// </summary>
    Task<DateTime> ConvertToUserLocalAsync(DateTime utcDateTime);

    /// <summary>
    /// Converts a user's local datetime to UTC.
    /// </summary>
    Task<DateTime> ConvertToUtcAsync(DateTime localDateTime);

    /// <summary>
    /// Checks if a given UTC datetime is "today" in the user's timezone.
    /// </summary>
    Task<bool> IsTodayAsync(DateTime utcDateTime);

    /// <summary>
    /// Checks if a given UTC datetime is overdue (before now) in the user's timezone.
    /// </summary>
    Task<bool> IsOverdueAsync(DateTime utcDateTime);
}
