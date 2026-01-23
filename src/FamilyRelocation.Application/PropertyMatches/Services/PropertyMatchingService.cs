using System.Text.Json;
using FamilyRelocation.Application.PropertyMatches.DTOs;
using FamilyRelocation.Domain.Entities;

namespace FamilyRelocation.Application.PropertyMatches.Services;

/// <summary>
/// Implementation of the property matching scoring algorithm.
///
/// Scoring breakdown:
/// - Budget: 30 points max
/// - Bedrooms: 20 points max
/// - Bathrooms: 15 points max
/// - City: 20 points max
/// - Features: 15 points max
/// Total: 100 points max
/// </summary>
public class PropertyMatchingService : IPropertyMatchingService
{
    private const int MaxBudgetScore = 30;
    private const int MaxBedroomsScore = 20;
    private const int MaxBathroomsScore = 15;
    private const int MaxCityScore = 20;
    private const int MaxFeaturesScore = 15;
    private const int MaxTotalScore = 100;

    public (int Score, MatchScoreBreakdownDto Details) CalculateMatchScore(Property property, HousingSearch housingSearch)
    {
        var preferences = housingSearch.Preferences;

        // Budget scoring
        var (budgetScore, budgetNotes) = CalculateBudgetScore(property, preferences);

        // Bedrooms scoring
        var (bedroomsScore, bedroomsNotes) = CalculateBedroomsScore(property, preferences);

        // Bathrooms scoring
        var (bathroomsScore, bathroomsNotes) = CalculateBathroomsScore(property, preferences);

        // City scoring
        var (cityScore, cityNotes) = CalculateCityScore(property, preferences);

        // Features scoring
        var (featuresScore, featuresNotes) = CalculateFeaturesScore(property, preferences);

        var totalScore = budgetScore + bedroomsScore + bathroomsScore + cityScore + featuresScore;

        var details = new MatchScoreBreakdownDto
        {
            BudgetScore = budgetScore,
            MaxBudgetScore = MaxBudgetScore,
            BudgetNotes = budgetNotes,
            BedroomsScore = bedroomsScore,
            MaxBedroomsScore = MaxBedroomsScore,
            BedroomsNotes = bedroomsNotes,
            BathroomsScore = bathroomsScore,
            MaxBathroomsScore = MaxBathroomsScore,
            BathroomsNotes = bathroomsNotes,
            CityScore = cityScore,
            MaxCityScore = MaxCityScore,
            CityNotes = cityNotes,
            FeaturesScore = featuresScore,
            MaxFeaturesScore = MaxFeaturesScore,
            FeaturesNotes = featuresNotes,
            TotalScore = totalScore,
            MaxTotalScore = MaxTotalScore
        };

        return (totalScore, details);
    }

    private (int Score, string? Notes) CalculateBudgetScore(Property property, Domain.ValueObjects.HousingPreferences? preferences)
    {
        if (preferences?.Budget == null)
        {
            // Give 1/3 of max for unset preferences (contributes to baseline score of 50)
            return (MaxBudgetScore / 3, "No budget specified");
        }

        var budget = preferences.Budget.Amount;
        var price = property.Price.Amount;

        if (price <= budget)
        {
            return (MaxBudgetScore, $"Within budget (${price:N0} <= ${budget:N0})");
        }

        // Calculate how much over budget (up to 20% over still gets partial points)
        var overPercentage = (price - budget) / budget;

        if (overPercentage <= 0.20m) // Up to 20% over
        {
            // Linear reduction from max to 0 as we go from 0% to 20% over
            var score = (int)(MaxBudgetScore * (1 - (overPercentage / 0.20m)));
            return (score, $"{overPercentage:P0} over budget (${price:N0} vs ${budget:N0})");
        }

        return (0, $"Significantly over budget ({overPercentage:P0} over)");
    }

    private (int Score, string? Notes) CalculateBedroomsScore(Property property, Domain.ValueObjects.HousingPreferences? preferences)
    {
        if (preferences?.MinBedrooms == null)
        {
            // Give 1/3 of max for unset preferences (contributes to baseline score of 50)
            return (MaxBedroomsScore / 3, "No bedroom preference specified");
        }

        var minBedrooms = preferences.MinBedrooms.Value;
        var propertyBedrooms = property.Bedrooms;

        if (propertyBedrooms >= minBedrooms)
        {
            return (MaxBedroomsScore, $"Meets requirement ({propertyBedrooms} >= {minBedrooms})");
        }

        if (propertyBedrooms == minBedrooms - 1)
        {
            return (MaxBedroomsScore / 2, $"1 bedroom short ({propertyBedrooms} vs {minBedrooms} needed)");
        }

        return (0, $"Not enough bedrooms ({propertyBedrooms} vs {minBedrooms} needed)");
    }

    private (int Score, string? Notes) CalculateBathroomsScore(Property property, Domain.ValueObjects.HousingPreferences? preferences)
    {
        if (preferences?.MinBathrooms == null)
        {
            // Give 1/3 of max for unset preferences (contributes to baseline score of 50)
            return (MaxBathroomsScore / 3, "No bathroom preference specified");
        }

        var minBathrooms = preferences.MinBathrooms.Value;
        var propertyBathrooms = property.Bathrooms;

        if (propertyBathrooms >= minBathrooms)
        {
            return (MaxBathroomsScore, $"Meets requirement ({propertyBathrooms} >= {minBathrooms})");
        }

        return (0, $"Not enough bathrooms ({propertyBathrooms} vs {minBathrooms} needed)");
    }

    private (int Score, string? Notes) CalculateCityScore(Property property, Domain.ValueObjects.HousingPreferences? preferences)
    {
        // For this CRM, we're focused on Union County (Union, Roselle Park, etc.)
        // Full points if in Union or Roselle Park
        var city = property.Address.City.ToLowerInvariant().Trim();

        if (city == "union" || city == "roselle park")
        {
            return (MaxCityScore, $"In target area ({property.Address.City})");
        }

        // Partial points for nearby areas
        var nearbyCities = new[] { "roselle", "kenilworth", "hillside", "elizabeth", "clark" };
        if (nearbyCities.Contains(city))
        {
            return (MaxCityScore / 2, $"Near target area ({property.Address.City})");
        }

        return (0, $"Outside target area ({property.Address.City})");
    }

    private (int Score, string? Notes) CalculateFeaturesScore(Property property, Domain.ValueObjects.HousingPreferences? preferences)
    {
        if (preferences?.RequiredFeatures == null || !preferences.RequiredFeatures.Any())
        {
            // Give 1/3 of max for unset preferences (contributes to baseline score of 50)
            return (MaxFeaturesScore / 3, "No feature preferences specified");
        }

        var requiredFeatures = preferences.RequiredFeatures.Select(f => f.ToLowerInvariant()).ToList();
        var propertyFeatures = property.Features.Select(f => f.ToLowerInvariant()).ToList();

        var matchedCount = requiredFeatures.Count(rf => propertyFeatures.Any(pf => pf.Contains(rf) || rf.Contains(pf)));
        var matchPercentage = (decimal)matchedCount / requiredFeatures.Count;

        var score = (int)(MaxFeaturesScore * matchPercentage);
        var notes = matchedCount == requiredFeatures.Count
            ? $"All {matchedCount} required features present"
            : $"{matchedCount}/{requiredFeatures.Count} required features present";

        return (score, notes);
    }

    public string SerializeMatchDetails(MatchScoreBreakdownDto details)
    {
        return JsonSerializer.Serialize(details, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public MatchScoreBreakdownDto? DeserializeMatchDetails(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<MatchScoreBreakdownDto>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}
