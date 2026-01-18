namespace FamilyRelocation.Application.Applicants.DTOs;

public record AddressDto
{
    public required string Street { get; init; }
    public string? Street2 { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string ZipCode { get; init; }

    /// <summary>
    /// Formatted full address: "123 Main St, Union, NJ 07083"
    /// </summary>
    public string FullAddress => $"{Street}, {City}, {State} {ZipCode}";
}
