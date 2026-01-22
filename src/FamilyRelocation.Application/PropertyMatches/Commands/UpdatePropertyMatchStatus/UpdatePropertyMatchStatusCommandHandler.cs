using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.PropertyMatches.DTOs;
using FamilyRelocation.Application.PropertyMatches.Services;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.PropertyMatches.Commands.UpdatePropertyMatchStatus;

public class UpdatePropertyMatchStatusCommandHandler : IRequestHandler<UpdatePropertyMatchStatusCommand, PropertyMatchDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPropertyMatchingService _matchingService;
    private readonly IActivityLogger _activityLogger;

    public UpdatePropertyMatchStatusCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPropertyMatchingService matchingService,
        IActivityLogger activityLogger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _matchingService = matchingService;
        _activityLogger = activityLogger;
    }

    public async Task<PropertyMatchDto> Handle(UpdatePropertyMatchStatusCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        // Parse status
        if (!Enum.TryParse<PropertyMatchStatus>(request.Status, true, out var newStatus))
        {
            throw new ValidationException($"Invalid status: {request.Status}. Valid values are: {string.Join(", ", Enum.GetNames<PropertyMatchStatus>())}");
        }

        // Load match with navigation properties
        var match = await _context.Set<PropertyMatch>()
            .Include(m => m.Property)
                .ThenInclude(p => p.Photos)
            .Include(m => m.HousingSearch)
                .ThenInclude(h => h.Applicant)
            .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

        if (match == null)
        {
            throw new NotFoundException(nameof(PropertyMatch), request.MatchId);
        }

        var oldStatus = match.Status;

        // Update status
        match.UpdateStatus(newStatus, userId, request.Notes);
        await _context.SaveChangesAsync(cancellationToken);

        var familyName = match.HousingSearch.Applicant?.Husband?.LastName ?? "Unknown";
        var propertyAddress = $"{match.Property.Address.Street}, {match.Property.Address.City}";

        await _activityLogger.LogAsync(
            "PropertyMatch",
            match.Id,
            "StatusUpdated",
            $"Match status changed from {oldStatus} to {newStatus} for {familyName} family at {propertyAddress}",
            cancellationToken);

        return match.ToDto(_matchingService);
    }
}
