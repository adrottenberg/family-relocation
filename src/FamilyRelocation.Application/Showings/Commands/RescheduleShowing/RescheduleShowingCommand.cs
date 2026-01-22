using FamilyRelocation.Application.Showings.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Showings.Commands.RescheduleShowing;

/// <summary>
/// Command to reschedule a showing.
/// </summary>
public record RescheduleShowingCommand(
    Guid ShowingId,
    DateOnly NewDate,
    TimeOnly NewTime) : IRequest<ShowingDto>;
