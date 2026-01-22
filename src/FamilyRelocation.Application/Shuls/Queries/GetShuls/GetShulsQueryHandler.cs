using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Common.Models;
using FamilyRelocation.Application.Shuls.DTOs;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Shuls.Queries.GetShuls;

public class GetShulsQueryHandler : IRequestHandler<GetShulsQuery, PaginatedList<ShulListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetShulsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ShulListDto>> Handle(GetShulsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Shul>().AsQueryable();

        // IgnoreQueryFilters if including inactive
        if (request.IncludeInactive)
        {
            query = query.IgnoreQueryFilters();
        }

        // Search by name or rabbi
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(searchLower) ||
                (s.Rabbi != null && s.Rabbi.ToLower().Contains(searchLower)));
        }

        // Filter by city
        if (!string.IsNullOrWhiteSpace(request.City))
        {
            query = query.Where(s => s.Address.City.ToLower() == request.City.ToLower());
        }

        // Filter by denomination
        if (!string.IsNullOrWhiteSpace(request.Denomination))
        {
            query = query.Where(s => s.Denomination != null &&
                s.Denomination.ToLower() == request.Denomination.ToLower());
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderBy(s => s.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => ShulMapper.ToListDto(s))
            .ToListAsync(cancellationToken);

        return new PaginatedList<ShulListDto>(items, totalCount, request.Page, request.PageSize);
    }
}
