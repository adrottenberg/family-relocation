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
/// Handles HousingPreferencesUpdated events.
/// Re-calculates match scores for existing matches and creates new matches for properties
/// that now meet the minimum threshold after preference changes.
/// </summary>
public class HousingPreferencesUpdatedHandler : INotificationHandler<DomainEventNotification<HousingPreferencesUpdated>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPropertyMatchingService _matchingService;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<HousingPreferencesUpdatedHandler> _logger;

    private const int MinimumMatchScore = 50;
    private const int HighScoreThreshold = 70;

    public HousingPreferencesUpdatedHandler(
        IApplicationDbContext context,
        IPropertyMatchingService matchingService,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<HousingPreferencesUpdatedHandler> logger)
    {
        _context = context;
        _matchingService = matchingService;
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(DomainEventNotification<HousingPreferencesUpdated> notification, CancellationToken cancellationToken)
    {
        var applicantId = notification.DomainEvent.ApplicantId;
        _logger.LogInformation("Processing HousingPreferencesUpdated event for applicant {ApplicantId}", applicantId);

        // Get the applicant and their housing search
        var applicant = await _context.Set<Applicant>()
            .FirstOrDefaultAsync(a => a.Id == applicantId, cancellationToken);

        if (applicant?.ActiveHousingSearch == null)
        {
            _logger.LogWarning("Applicant {ApplicantId} not found or has no housing search", applicantId);
            return;
        }

        var housingSearch = await _context.Set<HousingSearch>()
            .Include(hs => hs.Preferences)
            .FirstOrDefaultAsync(hs => hs.Id == applicant.ActiveHousingSearch!.Id, cancellationToken);

        if (housingSearch == null)
        {
            _logger.LogWarning("Housing search not found for applicant {ApplicantId}", applicantId);
            return;
        }

        // Only process if in Searching stage
        if (housingSearch.Stage != HousingSearchStage.Searching)
        {
            _logger.LogInformation(
                "Housing search for applicant {ApplicantId} is in stage {Stage}, skipping re-matching",
                applicantId, housingSearch.Stage);
            return;
        }

        var familyName = applicant.Husband?.LastName ?? "Unknown";
        var userId = _currentUserService.UserId ?? Guid.Empty;

        // Update existing matches - recalculate scores and remove low-scoring ones
        var existingMatches = await _context.Set<PropertyMatch>()
            .Where(pm => pm.HousingSearchId == housingSearch.Id)
            .ToListAsync(cancellationToken);

        var updatedCount = 0;
        var removedCount = 0;
        var matchesToRemove = new List<PropertyMatch>();

        foreach (var match in existingMatches)
        {
            var property = await _context.Set<Property>()
                .Include(p => p.Address)
                .FirstOrDefaultAsync(p => p.Id == match.PropertyId, cancellationToken);

            if (property == null) continue;

            var (newScore, details) = _matchingService.CalculateMatchScore(property, housingSearch);
            var matchDetails = _matchingService.SerializeMatchDetails(details);

            // Check if score falls below threshold
            if (newScore < MinimumMatchScore)
            {
                // Only remove if applicant hasn't shown interest (still in MatchIdentified status)
                if (match.Status == PropertyMatchStatus.MatchIdentified)
                {
                    matchesToRemove.Add(match);
                    _logger.LogInformation(
                        "Match {MatchId} for property {PropertyStreet} will be removed (score {Score} < {Threshold})",
                        match.Id, property.Address.Street, newScore, MinimumMatchScore);
                }
                else
                {
                    // Keep match but update score - applicant has shown interest
                    match.UpdateScore(newScore, matchDetails, userId);
                    _logger.LogInformation(
                        "Match {MatchId} score below threshold ({Score}) but kept due to status {Status}",
                        match.Id, newScore, match.Status);
                    updatedCount++;
                }
            }
            else if (match.MatchScore != newScore)
            {
                // Update the match with new score
                match.UpdateScore(newScore, matchDetails, userId);
                _logger.LogInformation(
                    "Match {MatchId} score updated from {OldScore} to {NewScore}",
                    match.Id, match.MatchScore, newScore);
                updatedCount++;
            }
        }

        // Remove low-scoring matches that haven't progressed
        foreach (var match in matchesToRemove)
        {
            _context.Remove(match);
            removedCount++;
        }

        if (updatedCount > 0 || removedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Updated {Updated} match scores, removed {Removed} low-scoring matches for {Family} family",
                updatedCount, removedCount, familyName);
        }

        // Find and create new matches for properties not yet matched
        // Exclude properties that were just removed due to low scores
        var removedPropertyIds = matchesToRemove.Select(m => m.PropertyId).ToHashSet();
        var activeProperties = await _context.Set<Property>()
            .Include(p => p.Address)
            .Where(p => p.Status == ListingStatus.Active)
            .ToListAsync(cancellationToken);

        var matchedPropertyIds = existingMatches
            .Where(m => !matchesToRemove.Contains(m))
            .Select(m => m.PropertyId)
            .ToHashSet();

        var newMatchesCreated = 0;
        var highScoreMatches = new List<(PropertyMatch Match, Property Property)>();

        foreach (var property in activeProperties.Where(p => !matchedPropertyIds.Contains(p.Id) && !removedPropertyIds.Contains(p.Id)))
        {
            var (score, details) = _matchingService.CalculateMatchScore(property, housingSearch);

            if (score < MinimumMatchScore) continue;

            var matchDetails = _matchingService.SerializeMatchDetails(details);
            var match = PropertyMatch.Create(
                housingSearch.Id,
                property.Id,
                score,
                matchDetails,
                isAutoMatched: true,
                userId);

            _context.Add(match);
            newMatchesCreated++;

            if (score >= HighScoreThreshold)
            {
                highScoreMatches.Add((match, property));
            }
        }

        if (newMatchesCreated > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Created {Count} new automatic matches after preference update for {Family} family",
                newMatchesCreated, familyName);
        }

        // Create reminders for new high-score matches
        foreach (var (match, property) in highScoreMatches)
        {
            await CreateHighScoreMatchReminder(match, property, familyName, cancellationToken);
        }

        _logger.LogInformation(
            "Completed preference update processing for {Family} family: {Updated} scores updated, {Removed} removed, {New} new matches created",
            familyName, updatedCount, removedCount, newMatchesCreated);
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
                Title: $"New match found for {familyName} Family (preferences updated)",
                DueDate: DateTime.UtcNow.Date.AddDays(1),
                EntityType: "PropertyMatch",
                EntityId: match.Id,
                Notes: $"After updating preferences, property at {property.Address.Street}, {property.Address.City} now scores {match.MatchScore}% match. Review and schedule showing if appropriate.",
                Priority: ReminderPriority.High
            );

            await _mediator.Send(command, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create reminder for match {MatchId}", match.Id);
        }
    }
}
