using FamilyRelocation.Application.Properties.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Properties.Commands.UpdateProperty;

public record UpdatePropertyCommand : IRequest<PropertyDto>
{
    public required Guid Id { get; init; }

    // Address
    public required string Street { get; init; }
    public string? Street2 { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string ZipCode { get; init; }

    // Property details
    public required decimal Price { get; init; }
    public required int Bedrooms { get; init; }
    public required decimal Bathrooms { get; init; }
    public int? SquareFeet { get; init; }
    public decimal? LotSize { get; init; }
    public int? YearBuilt { get; init; }
    public decimal? AnnualTaxes { get; init; }
    public List<string>? Features { get; init; }
    public string? MlsNumber { get; init; }
    public string? Notes { get; init; }
}
