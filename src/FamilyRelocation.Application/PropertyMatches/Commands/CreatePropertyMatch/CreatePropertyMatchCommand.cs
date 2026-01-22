using FamilyRelocation.Application.PropertyMatches.DTOs;
using MediatR;

namespace FamilyRelocation.Application.PropertyMatches.Commands.CreatePropertyMatch;

/// <summary>
/// Command to create a manual property match.
/// </summary>
public record CreatePropertyMatchCommand(
    Guid HousingSearchId,
    Guid PropertyId,
    string? Notes) : IRequest<PropertyMatchDto>;
