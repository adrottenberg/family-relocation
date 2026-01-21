namespace FamilyRelocation.Application.Properties.DTOs;

/// <summary>
/// Full property details for detail views.
/// </summary>
public record PropertyDto
{
    public required Guid Id { get; init; }
    public required PropertyAddressDto Address { get; init; }
    public required decimal Price { get; init; }
    public required int Bedrooms { get; init; }
    public required decimal Bathrooms { get; init; }
    public int? SquareFeet { get; init; }
    public decimal? LotSize { get; init; }
    public int? YearBuilt { get; init; }
    public decimal? AnnualTaxes { get; init; }
    public List<string> Features { get; init; } = new();
    public required string Status { get; init; }
    public string? MlsNumber { get; init; }
    public string? Notes { get; init; }
    public List<PropertyPhotoDto> Photos { get; init; } = new();
    public required DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
}

/// <summary>
/// Lightweight property data for list views.
/// </summary>
public record PropertyListDto
{
    public required Guid Id { get; init; }
    public required string Street { get; init; }
    public required string City { get; init; }
    public required decimal Price { get; init; }
    public required int Bedrooms { get; init; }
    public required decimal Bathrooms { get; init; }
    public int? SquareFeet { get; init; }
    public required string Status { get; init; }
    public string? MlsNumber { get; init; }
    public string? PrimaryPhotoUrl { get; init; }
}

/// <summary>
/// Address data for properties.
/// </summary>
public record PropertyAddressDto
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
/// Photo data for property images.
/// </summary>
public record PropertyPhotoDto
{
    public required Guid Id { get; init; }
    public required string Url { get; init; }
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public required DateTime UploadedAt { get; init; }
}
