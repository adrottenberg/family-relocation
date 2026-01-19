namespace FamilyRelocation.Application.Applicants.DTOs;

/// <summary>
/// Represents a physical address.
/// </summary>
public record AddressDto
{
    /// <summary>
    /// Primary street address line.
    /// </summary>
    public required string Street { get; init; }

    /// <summary>
    /// Secondary address line (apartment, suite, etc.).
    /// </summary>
    public string? Street2 { get; init; }

    /// <summary>
    /// City name.
    /// </summary>
    public required string City { get; init; }

    /// <summary>
    /// State abbreviation (e.g., "NJ").
    /// </summary>
    public required string State { get; init; }

    /// <summary>
    /// ZIP or postal code.
    /// </summary>
    public required string ZipCode { get; init; }

    /// <summary>
    /// Formatted full address: "123 Main St, Union, NJ 07083"
    /// </summary>
    public string FullAddress => $"{Street}, {City}, {State} {ZipCode}";
}
