using FamilyRelocation.Domain.Enums;

namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Value object representing housing search preferences
/// </summary>
public sealed record HousingPreferences
{
    public Money? Budget { get; }
    public int? MinBedrooms { get; }
    public decimal? MinBathrooms { get; }
    public List<string> RequiredFeatures { get; }
    public ShulProximityPreference? ShulProximity { get; }
    public MoveTimeline? MoveTimeline { get; }

    // Private parameterless constructor for EF Core
    private HousingPreferences()
    {
        RequiredFeatures = new List<string>();
    }

    public HousingPreferences(
        Money? budget = null,
        int? minBedrooms = null,
        decimal? minBathrooms = null,
        List<string>? requiredFeatures = null,
        ShulProximityPreference? shulProximity = null,
        MoveTimeline? moveTimeline = null)
    {
        Budget = budget;
        MinBedrooms = minBedrooms;
        MinBathrooms = minBathrooms;
        RequiredFeatures = requiredFeatures ?? new List<string>();
        ShulProximity = shulProximity;
        MoveTimeline = moveTimeline;
    }

    /// <summary>
    /// Create default preferences with no requirements
    /// </summary>
    public static HousingPreferences Default() => new();

    /// <summary>
    /// Check if any preferences have been specified
    /// </summary>
    public bool HasPreferences =>
        Budget != null ||
        MinBedrooms.HasValue ||
        MinBathrooms.HasValue ||
        RequiredFeatures.Count > 0 ||
        (ShulProximity != null && !ShulProximity.AnyShulAcceptable) ||
        MoveTimeline.HasValue;

    public override string ToString()
    {
        var parts = new List<string>();

        if (Budget != null)
            parts.Add($"Budget: {Budget}");

        if (MinBedrooms.HasValue)
            parts.Add($"{MinBedrooms}+ BR");

        if (MinBathrooms.HasValue)
            parts.Add($"{MinBathrooms}+ BA");

        if (RequiredFeatures.Count > 0)
            parts.Add($"{RequiredFeatures.Count} features");

        if (MoveTimeline.HasValue)
            parts.Add($"Timeline: {MoveTimeline}");

        return parts.Count > 0 ? string.Join(", ", parts) : "No preferences";
    }
}
