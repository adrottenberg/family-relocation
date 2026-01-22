using FamilyRelocation.Application.PropertyMatches.DTOs;
using MediatR;

namespace FamilyRelocation.Application.PropertyMatches.Queries.GetPropertyMatchById;

/// <summary>
/// Query to get a single property match by ID.
/// </summary>
public record GetPropertyMatchByIdQuery(Guid MatchId) : IRequest<PropertyMatchDto?>;
