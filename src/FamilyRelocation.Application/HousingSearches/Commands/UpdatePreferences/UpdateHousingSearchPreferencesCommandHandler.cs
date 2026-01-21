using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.HousingSearches.Commands.UpdatePreferences;

/// <summary>
/// Handles the UpdateHousingSearchPreferencesCommand to update housing preferences.
/// </summary>
public class UpdateHousingSearchPreferencesCommandHandler
    : IRequestHandler<UpdateHousingSearchPreferencesCommand, UpdateHousingSearchPreferencesResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateHousingSearchPreferencesCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateHousingSearchPreferencesResponse> Handle(
        UpdateHousingSearchPreferencesCommand command,
        CancellationToken cancellationToken)
    {
        var housingSearch = await _context.Set<HousingSearch>()
            .FirstOrDefaultAsync(hs => hs.Id == command.HousingSearchId && hs.IsActive, cancellationToken)
            ?? throw new NotFoundException("HousingSearch", command.HousingSearchId);

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to update preferences.");

        var preferences = command.Request.ToDto().ToDomain();
        housingSearch.UpdatePreferences(preferences, userId);

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateHousingSearchPreferencesResponse
        {
            HousingSearchId = housingSearch.Id,
            Preferences = housingSearch.Preferences?.ToDto() ?? new HousingPreferencesDto(),
            ModifiedDate = housingSearch.ModifiedDate
        };
    }
}
