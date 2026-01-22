using FamilyRelocation.Application.Shuls.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Shuls.Commands.UpdateShul;

public record UpdateShulCommand : IRequest<ShulDto>
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }

    // Address
    public required string Street { get; init; }
    public string? Street2 { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string ZipCode { get; init; }

    // Optional fields
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? Rabbi { get; init; }
    public string? Denomination { get; init; }
    public string? Website { get; init; }
    public string? Notes { get; init; }
}
