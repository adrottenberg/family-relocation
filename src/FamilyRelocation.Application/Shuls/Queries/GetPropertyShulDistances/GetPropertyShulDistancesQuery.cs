using FamilyRelocation.Application.Shuls.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Shuls.Queries.GetPropertyShulDistances;

public record GetPropertyShulDistancesQuery(Guid PropertyId) : IRequest<List<PropertyShulDistanceDto>>;
