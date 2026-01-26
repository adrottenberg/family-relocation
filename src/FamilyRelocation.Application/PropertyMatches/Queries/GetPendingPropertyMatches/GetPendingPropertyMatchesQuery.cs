using FamilyRelocation.Application.PropertyMatches.DTOs;
using MediatR;

namespace FamilyRelocation.Application.PropertyMatches.Queries.GetPendingPropertyMatches;

/// <summary>
/// Query to get all property matches that are pending scheduling (ShowingRequested status).
/// </summary>
public record GetPendingPropertyMatchesQuery() : IRequest<List<PropertyMatchListDto>>;
