using FamilyRelocation.Application.Showings.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Showings.Queries.GetShowings;

/// <summary>
/// Query to get showings with optional filters.
/// </summary>
public record GetShowingsQuery(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    string? Status = null,
    Guid? BrokerId = null,
    Guid? PropertyMatchId = null) : IRequest<List<ShowingListDto>>;
