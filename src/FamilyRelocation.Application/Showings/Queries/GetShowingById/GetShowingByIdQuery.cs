using FamilyRelocation.Application.Showings.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Showings.Queries.GetShowingById;

/// <summary>
/// Query to get a single showing by ID.
/// </summary>
public record GetShowingByIdQuery(Guid ShowingId) : IRequest<ShowingDto?>;
