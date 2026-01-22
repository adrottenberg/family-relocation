using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.PropertyMatches.DTOs;
using FamilyRelocation.Application.PropertyMatches.Services;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.PropertyMatches.Queries.GetPropertyMatchById;

public class GetPropertyMatchByIdQueryHandler : IRequestHandler<GetPropertyMatchByIdQuery, PropertyMatchDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IPropertyMatchingService _matchingService;

    public GetPropertyMatchByIdQueryHandler(IApplicationDbContext context, IPropertyMatchingService matchingService)
    {
        _context = context;
        _matchingService = matchingService;
    }

    public async Task<PropertyMatchDto?> Handle(GetPropertyMatchByIdQuery request, CancellationToken cancellationToken)
    {
        var match = await _context.Set<PropertyMatch>()
            .Include(m => m.Property)
                .ThenInclude(p => p.Photos)
            .Include(m => m.HousingSearch)
                .ThenInclude(h => h.Applicant)
            .FirstOrDefaultAsync(m => m.Id == request.MatchId, cancellationToken);

        return match?.ToDto(_matchingService);
    }
}
