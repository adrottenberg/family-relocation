using FamilyRelocation.Application.PropertyMatches.DTOs;
using MediatR;

namespace FamilyRelocation.Application.PropertyMatches.Commands.UpdatePropertyMatchStatus;

/// <summary>
/// Command to update a property match status.
/// </summary>
public record UpdatePropertyMatchStatusCommand(
    Guid MatchId,
    string Status,
    string? Notes,
    decimal? OfferAmount = null) : IRequest<PropertyMatchDto>;
