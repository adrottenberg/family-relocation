using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Common.Models;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Queries.GetApplicants;

public class GetApplicantsQueryHandler : IRequestHandler<GetApplicantsQuery, PaginatedList<ApplicantListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetApplicantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ApplicantListDto>> Handle(
        GetApplicantsQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        // Start with base query (exclude soft-deleted)
        var query = _context.Set<Applicant>()
            .Where(a => !a.IsDeleted);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(a =>
                a.Husband.FirstName.ToLower().Contains(search) ||
                a.Husband.LastName.ToLower().Contains(search) ||
                (a.Husband.Email != null && a.Husband.Email.ToLower().Contains(search)) ||
                (a.Wife != null && a.Wife.FirstName.ToLower().Contains(search)) ||
                (a.Wife != null && a.Wife.MaidenName != null && a.Wife.MaidenName.ToLower().Contains(search)));
        }

        // Apply board decision filter
        if (!string.IsNullOrWhiteSpace(request.BoardDecision))
        {
            if (request.BoardDecision.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(a => a.BoardReview == null || a.BoardReview.Decision == BoardDecision.Pending);
            }
            else if (Enum.TryParse<BoardDecision>(request.BoardDecision, ignoreCase: true, out var decision))
            {
                query = query.Where(a => a.BoardReview != null && a.BoardReview.Decision == decision);
            }
        }

        // Apply city filter
        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var city = request.City.Trim().ToLower();
            query = query.Where(a => a.Address != null && a.Address.City.ToLower() == city);
        }

        // Apply date range filters
        if (request.CreatedAfter.HasValue)
        {
            query = query.Where(a => a.CreatedDate >= request.CreatedAfter.Value);
        }

        if (request.CreatedBefore.HasValue)
        {
            query = query.Where(a => a.CreatedDate <= request.CreatedBefore.Value);
        }

        // Apply sorting
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and fetch
        var applicants = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var items = applicants.Select(a => a.ToListDto()).ToList();

        return new PaginatedList<ApplicantListDto>(items, totalCount, page, pageSize);
    }

    private static IQueryable<Applicant> ApplySorting(
        IQueryable<Applicant> query,
        string sortBy,
        string sortOrder)
    {
        var isDescending = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "familyname" or "name" => isDescending
                ? query.OrderByDescending(a => a.Husband.LastName).ThenByDescending(a => a.Husband.FirstName)
                : query.OrderBy(a => a.Husband.LastName).ThenBy(a => a.Husband.FirstName),

            "boardreviewdate" => isDescending
                ? query.OrderByDescending(a => a.BoardReview != null ? a.BoardReview.ReviewDate : (DateTime?)null)
                : query.OrderBy(a => a.BoardReview != null ? a.BoardReview.ReviewDate : (DateTime?)null),

            // Default: createdDate
            _ => isDescending
                ? query.OrderByDescending(a => a.CreatedDate)
                : query.OrderBy(a => a.CreatedDate)
        };
    }
}
