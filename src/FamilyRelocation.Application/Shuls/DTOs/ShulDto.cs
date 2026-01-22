namespace FamilyRelocation.Application.Shuls.DTOs;

/// <summary>
/// Full shul details for detail views.
/// </summary>
public record ShulDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required ShulAddressDto Address { get; init; }
    public ShulCoordinatesDto? Location { get; init; }
    public string? Rabbi { get; init; }
    public string? Denomination { get; init; }
    public string? Website { get; init; }
    public string? Notes { get; init; }
    public bool IsActive { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
}

/// <summary>
/// Lightweight shul data for list views.
/// </summary>
public record ShulListDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Street { get; init; }
    public required string City { get; init; }
    public string? Rabbi { get; init; }
    public string? Denomination { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// Address data for shuls.
/// </summary>
public record ShulAddressDto
{
    public required string Street { get; init; }
    public string? Street2 { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string ZipCode { get; init; }
    public string FullAddress => Street2 != null
        ? $"{Street}, {Street2}, {City}, {State} {ZipCode}"
        : $"{Street}, {City}, {State} {ZipCode}";
}

/// <summary>
/// Coordinates data for shuls.
/// </summary>
public record ShulCoordinatesDto
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}

/// <summary>
/// Walking distance from a property to a shul.
/// </summary>
public record PropertyShulDistanceDto
{
    public required Guid ShulId { get; init; }
    public required string ShulName { get; init; }
    public required double DistanceMiles { get; init; }
    public required int WalkingTimeMinutes { get; init; }
    public required DateTime CalculatedAt { get; init; }
}
