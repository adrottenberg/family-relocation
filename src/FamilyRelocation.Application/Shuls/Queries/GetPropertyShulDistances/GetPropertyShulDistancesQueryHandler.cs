using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Shuls.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Shuls.Queries.GetPropertyShulDistances;

public class GetPropertyShulDistancesQueryHandler : IRequestHandler<GetPropertyShulDistancesQuery, List<PropertyShulDistanceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPropertyShulDistancesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PropertyShulDistanceDto>> Handle(GetPropertyShulDistancesQuery request, CancellationToken cancellationToken)
    {
        var distances = await _context.Set<PropertyShulDistance>()
            .Where(d => d.PropertyId == request.PropertyId)
            .ToListAsync(cancellationToken);

        if (!distances.Any())
            return new List<PropertyShulDistanceDto>();

        var shulIds = distances.Select(d => d.ShulId).Distinct().ToList();
        var shuls = await _context.Set<Shul>()
            .Where(s => shulIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        return distances
            .Where(d => shuls.ContainsKey(d.ShulId))
            .Select(d => ShulMapper.ToDistanceDto(d, shuls[d.ShulId]))
            .OrderBy(d => d.WalkingTimeMinutes)
            .ToList();
    }
}
