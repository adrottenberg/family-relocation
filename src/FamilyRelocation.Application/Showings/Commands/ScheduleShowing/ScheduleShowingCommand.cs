using FamilyRelocation.Application.Showings.DTOs;
using MediatR;

namespace FamilyRelocation.Application.Showings.Commands.ScheduleShowing;

/// <summary>
/// Command to schedule a new showing.
/// </summary>
public record ScheduleShowingCommand(
    Guid PropertyMatchId,
    DateTime ScheduledDateTime,
    string? Notes,
    Guid? BrokerUserId) : IRequest<ShowingDto>;
