using FamilyRelocation.Application.Common;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.PropertyMatches.Services;
using FamilyRelocation.Application.Reminders.Commands.CreateReminder;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using FamilyRelocation.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// Alias for clarity
using ListingStatus = FamilyRelocation.Domain.Enums.ListingStatus;

namespace FamilyRelocation.Application.PropertyMatches.EventHandlers;

/// <summary>
/// Handles PropertyCreated events by matching the new property against all active housing searches.
/// Creates automatic matches for properties with score > 70.
/// Creates reminders for high-score matches (>= 85).
/// </summary>
public class PropertyCreatedHandler : INotificationHandler<DomainEventNotification<PropertyCreated>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPropertyMatchingService _matchingService;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PropertyCreatedHandler> _logger;

    private const int MinimumMatchScore = 70;
    private const int HighScoreThreshold = 70;

    public PropertyCreatedHandler(
        IApplicationDbContext context,
        IPropertyMatchingService matchingService,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<PropertyCreatedHandler> logger)
    {
        _context = context;
        _matchingService = matchingService;
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<PropertyCreated> notification, CancellationToken cancellationToken)
    {
        var propertyId = notification.DomainEvent.PropertyId;
        _logger.LogInformation("Processing PropertyCreated event for property {PropertyId}", propertyId);

        // Get the property
        var property = await _context.Set<Property>()
            .Include(p => p.Address)
            .FirstOrDefaultAsync(p => p.Id == propertyId, cancellationToken);

        if (property == null)
        {
            _logger.LogWarning("Property {PropertyId} not found", propertyId);
            return;
        }

        // Only match active properties
        if (property.Status != ListingStatus.Active)
        {
            _logger.LogInformation("Property {PropertyId} is not active, skipping matching", propertyId);
            return;
        }

        // Get all active housing searches (in Searching stage)
        var activeSearches = await _context.Set<HousingSearch>()
            .Include(hs => hs.Preferences)
            .Where(hs => hs.Stage == HousingSearchStage.Searching)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} active housing searches to match against", activeSearches.Count);

        var matchesCreated = 0;
        var highScoreMatches = new List<(PropertyMatch Match, HousingSearch Search)>();

        foreach (var search in activeSearches)
        {
            // Check if match already exists
            var existingMatch = await _context.Set<PropertyMatch>()
                .AnyAsync(pm => pm.HousingSearchId == search.Id && pm.PropertyId == propertyId, cancellationToken);

            if (existingMatch)
            {
                continue;
            }

            // Calculate match score
            var (score, details) = _matchingService.CalculateMatchScore(property, search);

            // Only create matches for scores >= minimum threshold
            if (score < MinimumMatchScore)
            {
                continue;
            }

            // Create the match
            var userId = _currentUserService.UserId ?? Guid.Empty;
            var matchDetails = _matchingService.SerializeMatchDetails(details);
            var match = PropertyMatch.Create(
                search.Id,
                propertyId,
                score,
                matchDetails,
                isAutoMatched: true,
                userId);

            _context.Add(match);
            matchesCreated++;

            if (score >= HighScoreThreshold)
            {
                highScoreMatches.Add((match, search));
            }
        }

        if (matchesCreated > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Created {Count} automatic matches for property {PropertyId}", matchesCreated, propertyId);
        }

        // Create reminders for high-score matches
        foreach (var (match, search) in highScoreMatches)
        {
            await CreateHighScoreMatchReminder(match, search, property, cancellationToken);
        }
    }

    private async Task CreateHighScoreMatchReminder(
        PropertyMatch match,
        HousingSearch search,
        Property property,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get applicant name for the reminder
            var applicant = await _context.Set<Applicant>()
                .Include(a => a.HousingSearches)
                .FirstOrDefaultAsync(a => a.HousingSearches.Any(hs => hs.Id == search.Id), cancellationToken);

            var familyName = applicant?.Husband?.LastName ?? "Unknown";

            var command = new CreateReminderCommand(
                Title: $"New high-score property match for {familyName} Family",
                DueDate: DateTime.UtcNow.Date.AddDays(1), // Due tomorrow
                EntityType: "PropertyMatch",
                EntityId: match.Id,
                Notes: $"Property at {property.Address.Street}, {property.Address.City} scored {match.MatchScore}% match. Review and schedule showing if appropriate.",
                Priority: ReminderPriority.High
            );

            await _mediator.Send(command, cancellationToken);
            _logger.LogInformation(
                "Created reminder for high-score match {MatchId} (score: {Score})",
                match.Id, match.MatchScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create reminder for match {MatchId}", match.Id);
            // Don't rethrow - the match was created successfully, reminder is nice-to-have
        }
    }
}
