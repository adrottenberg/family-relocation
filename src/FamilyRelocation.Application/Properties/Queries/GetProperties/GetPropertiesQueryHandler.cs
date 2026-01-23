using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Common.Models;
using FamilyRelocation.Application.Properties.DTOs;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Properties.Queries.GetProperties;

public class GetPropertiesQueryHandler : IRequestHandler<GetPropertiesQuery, PaginatedList<PropertyListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPropertiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<PropertyListDto>> Handle(GetPropertiesQuery query, CancellationToken ct)
    {
        // Normalize pagination (max 100 per page)
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var dbQuery = _context.Set<Property>().AsQueryable();

        // Search by address or MLS
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            dbQuery = dbQuery.Where(p =>
                p.Address.Street.ToLower().Contains(search) ||
                p.Address.City.ToLower().Contains(search) ||
                (p.MlsNumber != null && p.MlsNumber.ToLower().Contains(search)));
        }

        // Filter by status
        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<ListingStatus>(query.Status, true, out var status))
        {
            dbQuery = dbQuery.Where(p => p.Status == status);
        }

        // Filter by price range
        if (query.MinPrice.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.Price.Amount >= query.MinPrice.Value);
        }
        if (query.MaxPrice.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.Price.Amount <= query.MaxPrice.Value);
        }

        // Filter by minimum bedrooms
        if (query.MinBeds.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.Bedrooms >= query.MinBeds.Value);
        }

        // Filter by city
        if (!string.IsNullOrWhiteSpace(query.City))
        {
            dbQuery = dbQuery.Where(p => p.Address.City.ToLower() == query.City.ToLower());
        }

        // Sorting
        var sortOrder = query.SortOrder?.ToLower() == "asc" ? "asc" : "desc";
        dbQuery = query.SortBy?.ToLower() switch
        {
            "price" => sortOrder == "asc"
                ? dbQuery.OrderBy(p => p.Price.Amount)
                : dbQuery.OrderByDescending(p => p.Price.Amount),
            "beds" => sortOrder == "asc"
                ? dbQuery.OrderBy(p => p.Bedrooms)
                : dbQuery.OrderByDescending(p => p.Bedrooms),
            "createddate" => sortOrder == "asc"
                ? dbQuery.OrderBy(p => p.CreatedAt)
                : dbQuery.OrderByDescending(p => p.CreatedAt),
            _ => dbQuery.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await dbQuery.CountAsync(ct);

        var items = await dbQuery
            .Include(p => p.Photos)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => p.ToListDto())
            .ToListAsync(ct);

        return new PaginatedList<PropertyListDto>(items, totalCount, page, pageSize);
    }
}
