namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Shul proximity preference value object
/// Specifies preferred shuls and maximum walking distance
/// </summary>
public sealed record ShulProximityPreference
{
    public List<Guid> PreferredShulIds { get; init; }
    public double? MaxWalkingDistanceMiles { get; init; }
    public int? MaxWalkingTimeMinutes { get; init; }
    public bool AnyShulAcceptable { get; init; }

    // Private parameterless constructor for EF Core
    private ShulProximityPreference()
    {
        PreferredShulIds = new List<Guid>();
        AnyShulAcceptable = true;
    }

    public ShulProximityPreference(
        List<Guid>? preferredShulIds = null,
        double? maxWalkingDistanceMiles = null,
        int? maxWalkingTimeMinutes = null,
        bool anyShulAcceptable = true)
    {
        PreferredShulIds = preferredShulIds ?? new List<Guid>();
        MaxWalkingDistanceMiles = maxWalkingDistanceMiles;
        MaxWalkingTimeMinutes = maxWalkingTimeMinutes;
        AnyShulAcceptable = anyShulAcceptable || PreferredShulIds.Count == 0;
    }

    public static ShulProximityPreference NoPreference()
    {
        return new ShulProximityPreference(
            preferredShulIds: new List<Guid>(),
            maxWalkingDistanceMiles: null,
            maxWalkingTimeMinutes: null,
            anyShulAcceptable: true);
    }

    public static ShulProximityPreference WithMaxDistance(double maxMiles)
    {
        return new ShulProximityPreference(
            preferredShulIds: new List<Guid>(),
            maxWalkingDistanceMiles: maxMiles,
            anyShulAcceptable: true);
    }

    public static ShulProximityPreference ForSpecificShuls(List<Guid> shulIds, double? maxMiles = null)
    {
        return new ShulProximityPreference(
            preferredShulIds: shulIds,
            maxWalkingDistanceMiles: maxMiles,
            anyShulAcceptable: false);
    }

    public override string ToString()
    {
        if (AnyShulAcceptable && !MaxWalkingDistanceMiles.HasValue)
            return "No preference";

        var parts = new List<string>();

        if (PreferredShulIds.Count > 0)
            parts.Add($"{PreferredShulIds.Count} preferred shul(s)");

        if (MaxWalkingDistanceMiles.HasValue)
            parts.Add($"max {MaxWalkingDistanceMiles:F1} mi");

        if (MaxWalkingTimeMinutes.HasValue)
            parts.Add($"max {MaxWalkingTimeMinutes} min");

        return string.Join(", ", parts);
    }
}
