using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.PropertyMatches.DTOs;
using FamilyRelocation.Application.PropertyMatches.Services;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.PropertyMatches.Commands.CreatePropertyMatch;

public class CreatePropertyMatchCommandHandler : IRequestHandler<CreatePropertyMatchCommand, PropertyMatchDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPropertyMatchingService _matchingService;
    private readonly IActivityLogger _activityLogger;

    public CreatePropertyMatchCommandHandler(
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

    public async Task<PropertyMatchDto> Handle(CreatePropertyMatchCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        // Load housing search with applicant
        var housingSearch = await _context.Set<HousingSearch>()
            .Include(h => h.Applicant)
            .FirstOrDefaultAsync(h => h.Id == request.HousingSearchId && h.IsActive, cancellationToken);

        if (housingSearch == null)
        {
            throw new NotFoundException(nameof(HousingSearch), request.HousingSearchId);
        }

        // Load property with photos
        var property = await _context.Set<Property>()
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId && !p.IsDeleted, cancellationToken);

        if (property == null)
        {
            throw new NotFoundException(nameof(Property), request.PropertyId);
        }

        // Check if match already exists
        var existingMatch = await _context.Set<PropertyMatch>()
            .FirstOrDefaultAsync(m => m.HousingSearchId == request.HousingSearchId && m.PropertyId == request.PropertyId, cancellationToken);

        if (existingMatch != null)
        {
            throw new ValidationException("A match already exists between this housing search and property.");
        }

        // Calculate match score
        var (score, details) = _matchingService.CalculateMatchScore(property, housingSearch);
        var matchDetailsJson = _matchingService.SerializeMatchDetails(details);

        // Create the match (manual matches have IsAutoMatched = false)
        var match = PropertyMatch.Create(
            housingSearchId: request.HousingSearchId,
            propertyId: request.PropertyId,
            matchScore: score,
            matchDetails: matchDetailsJson,
            isAutoMatched: false,
            createdBy: userId,
            notes: request.Notes);

        _context.Add(match);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties for DTO
        var savedMatch = await _context.Set<PropertyMatch>()
            .Include(m => m.Property)
                .ThenInclude(p => p.Photos)
            .Include(m => m.HousingSearch)
                .ThenInclude(h => h.Applicant)
            .FirstAsync(m => m.Id == match.Id, cancellationToken);

        var familyName = savedMatch.HousingSearch.Applicant?.Husband?.LastName ?? "Unknown";
        var propertyAddress = $"{property.Address.Street}, {property.Address.City}";

        await _activityLogger.LogAsync(
            "PropertyMatch",
            match.Id,
            "Created",
            $"Manual match created between {familyName} family and property at {propertyAddress} (Score: {score}%)",
            cancellationToken);

        return savedMatch.ToDto(_matchingService);
    }
}
