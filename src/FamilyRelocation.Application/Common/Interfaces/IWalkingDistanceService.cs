namespace FamilyRelocation.Application.Common.Interfaces;

/// <summary>
/// Service for calculating walking distances between locations using OpenStreetMap/OSRM.
/// </summary>
public interface IWalkingDistanceService
{
    /// <summary>
    /// Calculates walking distance and time between two coordinates.
    /// </summary>
    /// <param name="fromLatitude">Origin latitude</param>
    /// <param name="fromLongitude">Origin longitude</param>
    /// <param name="toLatitude">Destination latitude</param>
    /// <param name="toLongitude">Destination longitude</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Walking distance result or null if calculation failed</returns>
    Task<WalkingDistanceResult?> GetWalkingDistanceAsync(
        double fromLatitude,
        double fromLongitude,
        double toLatitude,
        double toLongitude,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Geocodes an address to get coordinates.
    /// </summary>
    /// <param name="street">Street address</param>
    /// <param name="city">City</param>
    /// <param name="state">State code</param>
    /// <param name="zipCode">Zip code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Coordinates or null if geocoding failed</returns>
    Task<GeocodingResult?> GeocodeAddressAsync(
        string street,
        string city,
        string state,
        string zipCode,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a walking distance calculation.
/// </summary>
public record WalkingDistanceResult(
    double DistanceMiles,
    int WalkingTimeMinutes);

/// <summary>
/// Result of geocoding an address.
/// </summary>
public record GeocodingResult(
    double Latitude,
    double Longitude);
