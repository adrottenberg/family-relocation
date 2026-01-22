using FamilyRelocation.Application.Showings.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Showings.Queries.GetUpcomingShowings;

/// <summary>
/// Query to get upcoming showings (today and future).
/// </summary>
public record GetUpcomingShowingsQuery(int? Days = null) : IRequest<List<ShowingListDto>>;
