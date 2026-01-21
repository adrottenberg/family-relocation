using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Commands.UpdatePreferences;

/// <summary>
/// Handles the UpdatePreferencesCommand to update housing preferences for an applicant.
/// </summary>
public class UpdatePreferencesCommandHandler : IRequestHandler<UpdatePreferencesCommand, UpdatePreferencesResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the handler.
    /// </summary>
    public UpdatePreferencesCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <inheritdoc />
    public async Task<UpdatePreferencesResponse> Handle(UpdatePreferencesCommand command, CancellationToken cancellationToken)
    {
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearches)
            .FirstOrDefaultAsync(a => a.Id == command.ApplicantId, cancellationToken)
            ?? throw new NotFoundException("Applicant", command.ApplicantId);

        var housingSearch = applicant.ActiveHousingSearch
            ?? throw new NotFoundException("Active HousingSearch for Applicant", command.ApplicantId);

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated to update preferences.");

        var preferences = command.Request.ToDomain();
        housingSearch.UpdatePreferences(preferences, userId);

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdatePreferencesResponse
        {
            Preferences = housingSearch.Preferences?.ToDto() ?? new HousingPreferencesDto(),
            ModifiedDate = housingSearch.ModifiedDate
        };
    }
}
