using FamilyRelocation.Application.Properties.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Properties.Queries.GetPropertyById;

public record GetPropertyByIdQuery(Guid Id) : IRequest<PropertyDto?>;
