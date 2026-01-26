using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.PropertyMatches.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.PropertyMatches.Queries.GetPendingPropertyMatches;

/// <summary>
/// Handler for GetPendingPropertyMatchesQuery.
/// Returns all property matches with ShowingRequested status that don't have a scheduled showing.
/// </summary>
public class GetPendingPropertyMatchesQueryHandler : IRequestHandler<GetPendingPropertyMatchesQuery, List<PropertyMatchListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingPropertyMatchesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PropertyMatchListDto>> Handle(GetPendingPropertyMatchesQuery request, CancellationToken cancellationToken)
    {
        // Get all property matches that are ready to schedule
        // Include both ShowingRequested and ApplicantInterested (legacy data before status consolidation)
        var matches = await _context.Set<PropertyMatch>()
            .Include(m => m.Property)
                .ThenInclude(p => p.Photos)
            .Include(m => m.HousingSearch)
                .ThenInclude(h => h.Applicant)
            .Where(m => m.Status == PropertyMatchStatus.ShowingRequested ||
                        m.Status == PropertyMatchStatus.ApplicantInterested)
            .OrderBy(m => m.HousingSearch.Applicant.FamilyName)
            .ThenByDescending(m => m.MatchScore)
            .Take(200) // Limit for safety
            .ToListAsync(cancellationToken);

        // Get all showings for these matches
        var matchIds = matches.Select(m => m.Id).ToList();
        var showingsList = await _context.Set<Showing>()
            .Where(s => matchIds.Contains(s.PropertyMatchId))
            .Select(s => new
            {
                s.Id,
                s.PropertyMatchId,
                s.ScheduledDateTime,
                s.Status,
                s.BrokerUserId,
                s.Notes,
                s.CompletedAt
            })
            .ToListAsync(cancellationToken);

        // Group showings by PropertyMatchId
        var showingsByMatch = showingsList
            .GroupBy(s => s.PropertyMatchId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(s => s.ScheduledDateTime)
                      .Select(s => new MatchShowingDto
                      {
                          Id = s.Id,
                          ScheduledDateTime = s.ScheduledDateTime,
                          Status = s.Status.ToString(),
                          BrokerUserId = s.BrokerUserId,
                          Notes = s.Notes,
                          CompletedAt = s.CompletedAt
                      })
                      .ToList());

        // Filter out matches that already have a scheduled showing (future)
        var now = DateTime.UtcNow;
        var pendingMatches = matches
            .Where(m =>
            {
                if (!showingsByMatch.TryGetValue(m.Id, out var showings))
                    return true; // No showings at all - needs scheduling

                // Check if there's a scheduled showing in the future
                return !showings.Any(s =>
                    s.Status == "Scheduled" &&
                    s.ScheduledDateTime >= now);
            })
            .ToList();

        return pendingMatches.Select(m =>
        {
            showingsByMatch.TryGetValue(m.Id, out var showings);
            return m.ToListDto(showings);
        }).ToList();
    }
}
