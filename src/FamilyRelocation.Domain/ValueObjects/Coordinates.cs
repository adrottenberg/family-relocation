namespace FamilyRelocation.Domain.ValueObjects;

/// <summary>
/// Geographic coordinates value object with Haversine distance calculation
/// </summary>
public sealed record Coordinates
{
    public double Latitude { get; }
    public double Longitude { get; }

    // Private parameterless constructor for EF Core
    private Coordinates()
    {
        Latitude = 0;
        Longitude = 0;
    }

    public Coordinates(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));

        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Calculate straight-line distance in miles using Haversine formula
    /// </summary>
    public double DistanceToMiles(Coordinates other)
    {
        const double earthRadiusMiles = 3958.8;

        var lat1Rad = ToRadians(Latitude);
        var lat2Rad = ToRadians(other.Latitude);
        var deltaLat = ToRadians(other.Latitude - Latitude);
        var deltaLon = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusMiles * c;
    }

    /// <summary>
    /// Calculate straight-line distance in kilometers
    /// </summary>
    public double DistanceToKilometers(Coordinates other)
    {
        return DistanceToMiles(other) * 1.60934;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    public override string ToString() => $"{Latitude:F6}, {Longitude:F6}";
}
