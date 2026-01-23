using MediatR;

namespace FamilyRelocation.Application.HousingSearches.Commands.DeactivateHousingSearch;

/// <summary>
/// Command to deactivate a housing search.
/// Used to clean up duplicate or orphaned housing searches.
/// </summary>
public record DeactivateHousingSearchCommand(Guid HousingSearchId) : IRequest<Unit>;
