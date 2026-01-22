using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Showings.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Showings.Queries.GetUpcomingShowings;

public class GetUpcomingShowingsQueryHandler : IRequestHandler<GetUpcomingShowingsQuery, List<ShowingListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUpcomingShowingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ShowingListDto>> Handle(GetUpcomingShowingsQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var days = request.Days ?? 14; // Default to 2 weeks
        var endDate = today.AddDays(days);

        var showings = await _context.Set<Showing>()
            .Include(s => s.PropertyMatch)
                .ThenInclude(m => m.Property)
                    .ThenInclude(p => p.Photos)
            .Include(s => s.PropertyMatch)
                .ThenInclude(m => m.HousingSearch)
                    .ThenInclude(h => h.Applicant)
            .Where(s => s.Status == ShowingStatus.Scheduled)
            .Where(s => s.ScheduledDate >= today && s.ScheduledDate <= endDate)
            .OrderBy(s => s.ScheduledDate)
            .ThenBy(s => s.ScheduledTime)
            .Take(100)
            .ToListAsync(cancellationToken);

        return showings.Select(s => s.ToListDto()).ToList();
    }
}
