using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Showings.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Showings.Queries.GetShowings;

public class GetShowingsQueryHandler : IRequestHandler<GetShowingsQuery, List<ShowingListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetShowingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ShowingListDto>> Handle(GetShowingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Showing>()
            .Include(s => s.PropertyMatch)
                .ThenInclude(m => m.Property)
                    .ThenInclude(p => p.Photos)
            .Include(s => s.PropertyMatch)
                .ThenInclude(m => m.HousingSearch)
                    .ThenInclude(h => h.Applicant)
            .AsQueryable();

        // Apply filters
        if (request.FromDate.HasValue)
        {
            query = query.Where(s => s.ScheduledDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(s => s.ScheduledDate <= request.ToDate.Value);
        }

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ShowingStatus>(request.Status, true, out var status))
        {
            query = query.Where(s => s.Status == status);
        }

        if (request.BrokerId.HasValue)
        {
            query = query.Where(s => s.BrokerUserId == request.BrokerId.Value);
        }

        if (request.PropertyMatchId.HasValue)
        {
            query = query.Where(s => s.PropertyMatchId == request.PropertyMatchId.Value);
        }

        var showings = await query
            .OrderBy(s => s.ScheduledDate)
            .ThenBy(s => s.ScheduledTime)
            .Take(200) // Limit for safety
            .ToListAsync(cancellationToken);

        return showings.Select(s => s.ToListDto()).ToList();
    }
}
