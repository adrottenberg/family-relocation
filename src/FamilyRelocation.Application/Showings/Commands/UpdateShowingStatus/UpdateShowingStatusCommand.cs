using FamilyRelocation.Application.Showings.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Showings.Commands.UpdateShowingStatus;

/// <summary>
/// Command to update a showing status (complete, cancel, no-show).
/// </summary>
public record UpdateShowingStatusCommand(
    Guid ShowingId,
    string Status,
    string? Notes) : IRequest<ShowingDto>;
