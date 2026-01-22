using System.Net.Http.Json;
using System.Text.Json;
using FamilyRelocation.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilyRelocation.Infrastructure.Services;

/// <summary>
/// Walking distance service using OpenStreetMap/OSRM public API.
/// Uses Nominatim for geocoding and OSRM for routing.
/// Note: The public OSRM demo server only supports driving profile,
/// so we use driving route distance and calculate walking time at 4 mph.
/// </summary>
public class OsrmWalkingDistanceService : IWalkingDistanceService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OsrmWalkingDistanceService> _logger;

    // Public OSRM demo server - only supports driving profile
    private const string OsrmBaseUrl = "https://router.project-osrm.org";
    // Nominatim for geocoding
    private const string NominatimBaseUrl = "https://nominatim.openstreetmap.org";
    // Walking speed: 4 mph = 15 minutes per mile
    private const double WalkingMinutesPerMile = 15.0;

    public OsrmWalkingDistanceService(
        HttpClient httpClient,
        ILogger<OsrmWalkingDistanceService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Set required User-Agent for Nominatim (they require it)
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("FamilyRelocation/1.0");
    }

    public async Task<WalkingDistanceResult?> GetWalkingDistanceAsync(
        double fromLatitude,
        double fromLongitude,
        double toLatitude,
        double toLongitude,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // OSRM public demo server only supports driving profile
            // We use driving route for distance (follows roads) and calculate walking time at 4 mph
            var url = $"{OsrmBaseUrl}/route/v1/driving/{fromLongitude},{fromLatitude};{toLongitude},{toLatitude}?overview=false";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "OSRM API returned {StatusCode} for route from ({FromLat},{FromLon}) to ({ToLat},{ToLon})",
                    response.StatusCode, fromLatitude, fromLongitude, toLatitude, toLongitude);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<OsrmRouteResponse>(cancellationToken: cancellationToken);

            if (result?.Code != "Ok" || result.Routes == null || result.Routes.Length == 0)
            {
                _logger.LogWarning(
                    "OSRM returned no valid route from ({FromLat},{FromLon}) to ({ToLat},{ToLon}). Code: {Code}",
                    fromLatitude, fromLongitude, toLatitude, toLongitude, result?.Code);
                return null;
            }

            var route = result.Routes[0];

            // Distance is in meters, convert to miles (1 mile = 1609.34 meters)
            var distanceMiles = route.Distance / 1609.34;

            // Calculate walking time at 4 mph (15 minutes per mile)
            var walkingTimeMinutes = (int)Math.Ceiling(distanceMiles * WalkingMinutesPerMile);

            return new WalkingDistanceResult(
                DistanceMiles: Math.Round(distanceMiles, 2),
                WalkingTimeMinutes: walkingTimeMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error calculating walking distance from ({FromLat},{FromLon}) to ({ToLat},{ToLon})",
                fromLatitude, fromLongitude, toLatitude, toLongitude);
            return null;
        }
    }

    public async Task<GeocodingResult?> GeocodeAddressAsync(
        string street,
        string city,
        string state,
        string zipCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Build the address query
            var query = Uri.EscapeDataString($"{street}, {city}, {state} {zipCode}, USA");
            var url = $"{NominatimBaseUrl}/search?q={query}&format=json&limit=1";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Nominatim API returned {StatusCode} for address: {Street}, {City}, {State} {Zip}",
                    response.StatusCode, street, city, state, zipCode);
                return null;
            }

            var results = await response.Content.ReadFromJsonAsync<NominatimResult[]>(cancellationToken: cancellationToken);

            if (results == null || results.Length == 0)
            {
                _logger.LogWarning(
                    "Nominatim returned no results for address: {Street}, {City}, {State} {Zip}",
                    street, city, state, zipCode);
                return null;
            }

            var result = results[0];

            if (!double.TryParse(result.Lat, out var latitude) ||
                !double.TryParse(result.Lon, out var longitude))
            {
                _logger.LogWarning(
                    "Failed to parse coordinates from Nominatim response for address: {Street}, {City}, {State} {Zip}",
                    street, city, state, zipCode);
                return null;
            }

            return new GeocodingResult(latitude, longitude);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error geocoding address: {Street}, {City}, {State} {Zip}",
                street, city, state, zipCode);
            return null;
        }
    }

    // OSRM API response models
    private record OsrmRouteResponse(
        string Code,
        OsrmRoute[]? Routes);

    private record OsrmRoute(
        double Distance,
        double Duration);

    // Nominatim API response model
    private record NominatimResult(
        string Lat,
        string Lon,
        string DisplayName);
}
