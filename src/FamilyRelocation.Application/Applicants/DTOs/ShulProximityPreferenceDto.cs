namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// DTO for shul proximity preferences.
/// </summary>
public class ShulProximityPreferenceDto
{
    /// <summary>
    /// List of preferred shul IDs (optional).
    /// </summary>
    public List<Guid>? PreferredShulIds { get; init; }

    /// <summary>
    /// Maximum walking distance in miles.
    /// </summary>
    public double? MaxWalkingDistanceMiles { get; init; }

    /// <summary>
    /// Maximum walking time in minutes.
    /// </summary>
    public int? MaxWalkingTimeMinutes { get; init; }

    /// <summary>
    /// Whether any shul within distance is acceptable (default: true).
    /// </summary>
    public bool AnyShulAcceptable { get; init; } = true;
}
