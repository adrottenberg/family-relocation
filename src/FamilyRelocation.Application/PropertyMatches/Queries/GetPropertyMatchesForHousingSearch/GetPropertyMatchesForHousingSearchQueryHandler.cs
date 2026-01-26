using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.PropertyMatches.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.PropertyMatches.Queries.GetPropertyMatchesForHousingSearch;

public class GetPropertyMatchesForHousingSearchQueryHandler : IRequestHandler<GetPropertyMatchesForHousingSearchQuery, List<PropertyMatchListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPropertyMatchesForHousingSearchQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PropertyMatchListDto>> Handle(GetPropertyMatchesForHousingSearchQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<PropertyMatch>()
            .Include(m => m.Property)
                .ThenInclude(p => p.Photos)
            .Include(m => m.HousingSearch)
                .ThenInclude(h => h.Applicant)
            .Where(m => m.HousingSearchId == request.HousingSearchId);

        // Filter by status if provided
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<PropertyMatchStatus>(request.Status, true, out var status))
        {
            query = query.Where(m => m.Status == status);
        }

        var matches = await query
            .OrderByDescending(m => m.MatchScore)
            .ThenByDescending(m => m.CreatedAt)
            .Take(100) // Limit for safety
            .ToListAsync(cancellationToken);

        // Get all showings for these matches (all statuses)
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

        return matches.Select(m =>
        {
            showingsByMatch.TryGetValue(m.Id, out var showings);
            return m.ToListDto(showings);
        }).ToList();
    }
}
