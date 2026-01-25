using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.PropertyMatches.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.PropertyMatches.Queries.GetPropertyMatchesForProperty;

public class GetPropertyMatchesForPropertyQueryHandler : IRequestHandler<GetPropertyMatchesForPropertyQuery, List<PropertyMatchListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPropertyMatchesForPropertyQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PropertyMatchListDto>> Handle(GetPropertyMatchesForPropertyQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<PropertyMatch>()
            .Include(m => m.Property)
                .ThenInclude(p => p.Photos)
            .Include(m => m.HousingSearch)
                .ThenInclude(h => h.Applicant)
            .Where(m => m.PropertyId == request.PropertyId);

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

        // Get scheduled showings for these matches (only Scheduled status, not completed/cancelled)
        var matchIds = matches.Select(m => m.Id).ToList();
        var scheduledShowings = await _context.Set<Showing>()
            .Where(s => matchIds.Contains(s.PropertyMatchId) && s.Status == ShowingStatus.Scheduled)
            .Select(s => new { s.PropertyMatchId, s.ScheduledDate, s.ScheduledTime })
            .ToDictionaryAsync(s => s.PropertyMatchId, cancellationToken);

        return matches.Select(m =>
        {
            scheduledShowings.TryGetValue(m.Id, out var showing);
            return m.ToListDto(showing?.ScheduledDate, showing?.ScheduledTime);
        }).ToList();
    }
}
