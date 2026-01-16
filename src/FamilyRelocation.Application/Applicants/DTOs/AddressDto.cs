namespace FamilyRelocation.Application.Applicants.DTOs;

public record AddressDto
{
    public required string Street { get; init; }
    public string? Street2 { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string ZipCode { get; init; }
}
