using MediatR;

namespace FamilyRelocation.Application.PropertyMatches.Commands.RequestShowing;

/// <summary>
/// Command to request showings for one or more property matches.
/// Sets status to ShowingRequested.
/// </summary>
public record RequestShowingCommand(List<Guid> MatchIds) : IRequest<int>;
