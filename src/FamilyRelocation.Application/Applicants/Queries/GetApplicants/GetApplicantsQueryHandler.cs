using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Common.Models;
using FamilyRelocation.Domain.Entities;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.Applicants.Queries.GetApplicants;

/// <summary>
/// Handles the GetApplicantsQuery to retrieve a paginated list of applicants.
/// </summary>
public class GetApplicantsQueryHandler : IRequestHandler<GetApplicantsQuery, PaginatedList<ApplicantListDto>>
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the handler.
    /// </summary>
    public GetApplicantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<PaginatedList<ApplicantListDto>> Handle(
        GetApplicantsQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        // Start with base query (exclude soft-deleted, include HousingSearches)
        var query = _context.Set<Applicant>()
            .Include(a => a.HousingSearches)
            .Where(a => !a.IsDeleted);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            // Strip non-digits for phone matching
            var phoneSearch = new string(request.Search.Where(char.IsDigit).ToArray());

            query = query.Where(a =>
                // Name search
                a.Husband.FirstName.ToLower().Contains(search) ||
                a.Husband.LastName.ToLower().Contains(search) ||
                (a.Wife != null && a.Wife.FirstName.ToLower().Contains(search)) ||
                (a.Wife != null && a.Wife.MaidenName != null && a.Wife.MaidenName.ToLower().Contains(search)) ||
                // Email search
                (a.Husband.Email != null && a.Husband.Email.ToLower().Contains(search)) ||
                (a.Wife != null && a.Wife.Email != null && a.Wife.Email.ToLower().Contains(search)) ||
                // Phone search (if search contains at least 3 digits)
                (phoneSearch.Length >= 3 && a.Husband.PhoneNumbers.Any(p => p.Number.Contains(phoneSearch))) ||
                (phoneSearch.Length >= 3 && a.Wife != null && a.Wife.PhoneNumbers.Any(p => p.Number.Contains(phoneSearch))));
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

        // Apply stage filter (filters by active housing search stage)
        if (!string.IsNullOrWhiteSpace(request.Stage))
        {
            if (Enum.TryParse<HousingSearchStage>(request.Stage, ignoreCase: true, out var stage))
            {
                query = query.Where(a => a.HousingSearches.Any(hs => hs.IsActive && hs.Stage == stage));
            }
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
