namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// Stores user-specific settings such as timezone preferences.
/// </summary>
public class UserSettings
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TimeZoneId { get; private set; } = "America/New_York";
    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }

    private UserSettings() { }

    /// <summary>
    /// Creates new user settings with default or specified timezone.
    /// </summary>
    public static UserSettings Create(Guid userId, string? timeZoneId = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));

        var effectiveTimeZoneId = timeZoneId ?? "America/New_York";
        ValidateTimeZoneId(effectiveTimeZoneId);

        return new UserSettings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TimeZoneId = effectiveTimeZoneId,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the user's timezone preference.
    /// </summary>
    public void UpdateTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            throw new ArgumentException("Timezone ID is required", nameof(timeZoneId));

        ValidateTimeZoneId(timeZoneId);

        TimeZoneId = timeZoneId;
        ModifiedAt = DateTime.UtcNow;
    }

    private static void ValidateTimeZoneId(string timeZoneId)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ArgumentException($"Invalid timezone ID: {timeZoneId}", nameof(timeZoneId));
        }
    }

    /// <summary>
    /// Gets the TimeZoneInfo for the user's timezone.
    /// </summary>
    public TimeZoneInfo GetTimeZoneInfo()
    {
        return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
    }
}
