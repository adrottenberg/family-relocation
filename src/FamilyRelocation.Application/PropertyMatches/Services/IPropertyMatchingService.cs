using FamilyRelocation.Application.PropertyMatches.DTOs;
using FamilyRelocation.Domain.Entities;

namespace FamilyRelocation.Application.PropertyMatches.Services;

/// <summary>
/// Service for calculating property match scores.
/// </summary>
public interface IPropertyMatchingService
{
    /// <summary>
    /// Calculates the match score between a property and housing search preferences.
    /// </summary>
    (int Score, MatchScoreBreakdownDto Details) CalculateMatchScore(Property property, HousingSearch housingSearch);

    /// <summary>
    /// Serializes match details to JSON for storage.
    /// </summary>
    string SerializeMatchDetails(MatchScoreBreakdownDto details);

    /// <summary>
    /// Deserializes match details from JSON.
    /// </summary>
    MatchScoreBreakdownDto? DeserializeMatchDetails(string? json);
}
