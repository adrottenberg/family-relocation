using FamilyRelocation.Application.AuditLogs.DTOs;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Common.Models;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.AuditLogs.Queries.GetApplicantFullAuditLogs;

/// <summary>
/// Handles the GetApplicantFullAuditLogsQuery to retrieve audit logs for an applicant
/// including their housing search history.
/// </summary>
public class GetApplicantFullAuditLogsQueryHandler : IRequestHandler<GetApplicantFullAuditLogsQuery, PaginatedList<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetApplicantFullAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AuditLogDto>> Handle(
        GetApplicantFullAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        // Find the housing search ID(s) for this applicant
        var housingSearchIds = await _context.Set<HousingSearch>()
            .Where(h => h.ApplicantId == request.ApplicantId)
            .Select(h => h.Id)
            .ToListAsync(cancellationToken);

        // Build a list of entity filters: Applicant + all HousingSearches
        var entityFilters = new List<(string EntityType, Guid EntityId)>
        {
            ("Applicant", request.ApplicantId)
        };

        foreach (var hsId in housingSearchIds)
        {
            entityFilters.Add(("HousingSearch", hsId));
        }

        // Query audit logs that match any of the entity filters
        var query = _context.Set<AuditLogEntry>()
            .Where(a => entityFilters.Any(f => a.EntityType == f.EntityType && a.EntityId == f.EntityId));

        // Order by timestamp descending (most recent first)
        query = query.OrderByDescending(a => a.Timestamp);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and fetch
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var dtos = items.Select(a => a.ToDto()).ToList();

        return new PaginatedList<AuditLogDto>(dtos, totalCount, page, pageSize);
    }
}
