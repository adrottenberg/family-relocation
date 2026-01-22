using MediatR;

namespace FamilyRelocation.Application.PropertyMatches.Commands.DeletePropertyMatch;

/// <summary>
/// Command to delete a property match.
/// </summary>
public record DeletePropertyMatchCommand(Guid MatchId) : IRequest;
