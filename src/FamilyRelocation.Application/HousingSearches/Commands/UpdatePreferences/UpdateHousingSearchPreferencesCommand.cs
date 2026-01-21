using FamilyRelocation.Application.Applicants.DTOs;
using MediatR;

namespace FamilyRelocation.Application.HousingSearches.Commands.UpdatePreferences;

/// <summary>
/// Command to update housing preferences for a housing search.
/// </summary>
/// <param name="HousingSearchId">The housing search ID.</param>
/// <param name="Request">The preferences update request.</param>
public record UpdateHousingSearchPreferencesCommand(
    Guid HousingSearchId,
    UpdateHousingSearchPreferencesRequest Request) : IRequest<UpdateHousingSearchPreferencesResponse>;

/// <summary>
/// Request body for updating housing search preferences.
/// </summary>
public class UpdateHousingSearchPreferencesRequest
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
    /// </summary>
    public string? MoveTimeline { get; init; }

    /// <summary>
    /// Convert to HousingPreferencesDto for domain conversion.
    /// </summary>
    public HousingPreferencesDto ToDto() => new()
    {
        BudgetAmount = BudgetAmount,
        MinBedrooms = MinBedrooms,
        MinBathrooms = MinBathrooms,
        RequiredFeatures = RequiredFeatures,
        ShulProximity = ShulProximity,
        MoveTimeline = MoveTimeline
    };
}

/// <summary>
/// Response returned after updating housing search preferences.
/// </summary>
public class UpdateHousingSearchPreferencesResponse
{
    /// <summary>
    /// The housing search ID.
    /// </summary>
    public required Guid HousingSearchId { get; init; }

    /// <summary>
    /// The updated housing preferences.
    /// </summary>
    public required HousingPreferencesDto Preferences { get; init; }

    /// <summary>
    /// When the preferences were last modified.
    /// </summary>
    public required DateTime ModifiedDate { get; init; }
}
