using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Showings.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Showings.Queries.GetShowingById;

public class GetShowingByIdQueryHandler : IRequestHandler<GetShowingByIdQuery, ShowingDto?>
{
    private readonly IApplicationDbContext _context;

    public GetShowingByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ShowingDto?> Handle(GetShowingByIdQuery request, CancellationToken cancellationToken)
    {
        var showing = await _context.Set<Showing>()
            .Include(s => s.PropertyMatch)
                .ThenInclude(m => m.Property)
                    .ThenInclude(p => p.Photos)
            .Include(s => s.PropertyMatch)
                .ThenInclude(m => m.HousingSearch)
                    .ThenInclude(h => h.Applicant)
            .FirstOrDefaultAsync(s => s.Id == request.ShowingId, cancellationToken);

        return showing?.ToDto();
    }
}
