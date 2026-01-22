using FamilyRelocation.Application.PropertyMatches.DTOs;
using MediatR;

namespace FamilyRelocation.Application.PropertyMatches.Queries.GetPropertyMatchesForProperty;

/// <summary>
/// Query to get all property matches for a property (interested families).
/// </summary>
public record GetPropertyMatchesForPropertyQuery(
    Guid PropertyId,
    string? Status = null) : IRequest<List<PropertyMatchListDto>>;
