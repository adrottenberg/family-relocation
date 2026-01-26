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

using ListingStatus = FamilyRelocation.Domain.Enums.ListingStatus;

namespace FamilyRelocation.Application.PropertyMatches.EventHandlers;

/// <summary>
/// Handles HousingSearchStageChanged events.
/// When a housing search moves to "Searching" stage, matches all active properties against it.
/// </summary>
public class HousingSearchStageChangedHandler : INotificationHandler<DomainEventNotification<HousingSearchStageChanged>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPropertyMatchingService _matchingService;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<HousingSearchStageChangedHandler> _logger;

    private const int MinimumMatchScore = 50;
    private const int HighScoreThreshold = 70;

    public HousingSearchStageChangedHandler(
        IApplicationDbContext context,
        IPropertyMatchingService matchingService,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<HousingSearchStageChangedHandler> logger)
    {
        _context = context;
        _matchingService = matchingService;
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<HousingSearchStageChanged> notification, CancellationToken cancellationToken)
    {
        var housingSearchId = notification.DomainEvent.HousingSearchId;
        var newStage = notification.DomainEvent.NewStage;
        var oldStage = notification.DomainEvent.OldStage;

        _logger.LogInformation(
            "Processing HousingSearchStageChanged event for search {HousingSearchId}: {OldStage} -> {NewStage}",
            housingSearchId, oldStage, newStage);

        // Only match when entering the Searching stage
        if (newStage != HousingSearchStage.Searching)
        {
            _logger.LogInformation("New stage is {Stage}, skipping property matching", newStage);
            return;
        }

        // Get the housing search with preferences
        var housingSearch = await _context.Set<HousingSearch>()
            .Include(hs => hs.Preferences)
            .FirstOrDefaultAsync(hs => hs.Id == housingSearchId, cancellationToken);

        if (housingSearch == null)
        {
            _logger.LogWarning("Housing search {HousingSearchId} not found", housingSearchId);
            return;
        }

        // Get applicant for logging and reminders
        var applicant = await _context.Set<Applicant>()
            .Include(a => a.HousingSearches)
            .FirstOrDefaultAsync(a => a.HousingSearches.Any(hs => hs.Id == housingSearchId), cancellationToken);

        var familyName = applicant?.Husband?.LastName ?? "Unknown";

        // Get all active properties
        var activeProperties = await _context.Set<Property>()
            .Include(p => p.Address)
            .Where(p => p.Status == ListingStatus.Active)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Matching {Count} active properties against housing search for {Family} family",
            activeProperties.Count, familyName);

        var matchesCreated = 0;
        var highScoreMatches = new List<(PropertyMatch Match, Property Property)>();

        foreach (var property in activeProperties)
        {
            // Check if match already exists
            var existingMatch = await _context.Set<PropertyMatch>()
                .AnyAsync(pm => pm.HousingSearchId == housingSearchId && pm.PropertyId == property.Id, cancellationToken);

            if (existingMatch)
            {
                continue;
            }

            // Calculate match score
            var (score, details) = _matchingService.CalculateMatchScore(property, housingSearch);

            // Only create matches for scores >= minimum threshold
            if (score < MinimumMatchScore)
            {
                continue;
            }

            // Create the match
            var userId = _currentUserService.UserId ?? Guid.Empty;
            var matchDetails = _matchingService.SerializeMatchDetails(details);
            var match = PropertyMatch.Create(
                housingSearchId,
                property.Id,
                score,
                matchDetails,
                isAutoMatched: true,
                userId);

            _context.Add(match);
            matchesCreated++;

            if (score >= HighScoreThreshold)
            {
                highScoreMatches.Add((match, property));
            }
        }

        if (matchesCreated > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Created {Count} automatic matches for {Family} family's housing search",
                matchesCreated, familyName);
        }

        // Create reminders for high-score matches
        foreach (var (match, property) in highScoreMatches)
        {
            await CreateHighScoreMatchReminder(match, property, familyName, cancellationToken);
        }
    }

    private async Task CreateHighScoreMatchReminder(
        PropertyMatch match,
        Property property,
        string familyName,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateReminderCommand(
                Title: $"High-score property match for {familyName} Family",
                DueDateTime: DateTime.UtcNow.Date.AddDays(1).AddHours(9), // Due tomorrow 9 AM UTC
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
        }
    }
}
