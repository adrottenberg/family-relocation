namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// DTO for housing preferences when creating an applicant.
/// </summary>
public class HousingPreferencesDto
{
    /// <summary>
    /// Maximum budget amount in USD.
    /// </summary>
    public decimal? BudgetAmount { get; init; }

    /// <summary>
    /// Minimum number of bedrooms required.
    /// </summary>
    public int? MinBedrooms { get; init; }

    /// <summary>
    /// Minimum number of bathrooms required.
    /// </summary>
    public decimal? MinBathrooms { get; init; }

    /// <summary>
    /// List of required features (e.g., "basement", "garage", "yard").
    /// </summary>
    public List<string>? RequiredFeatures { get; init; }

    /// <summary>
    /// Shul proximity preferences.
    /// </summary>
    public ShulProximityPreferenceDto? ShulProximity { get; init; }

    /// <summary>
    /// When the family plans to move.
    /// Valid values: Immediate, ShortTerm, MediumTerm, LongTerm, Extended, Flexible, NotSure, Never
    /// </summary>
    public string? MoveTimeline { get; init; }
}
