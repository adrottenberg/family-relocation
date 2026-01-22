using FamilyRelocation.Application.PropertyMatches.DTOs;
using MediatR;

namespace FamilyRelocation.Application.PropertyMatches.Queries.GetPropertyMatchesForHousingSearch;

/// <summary>
/// Query to get all property matches for a housing search.
/// </summary>
public record GetPropertyMatchesForHousingSearchQuery(
    Guid HousingSearchId,
    string? Status = null) : IRequest<List<PropertyMatchListDto>>;
