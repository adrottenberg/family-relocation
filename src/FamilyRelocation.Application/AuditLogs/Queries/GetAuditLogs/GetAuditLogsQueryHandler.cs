using FamilyRelocation.Application.AuditLogs.DTOs;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Common.Models;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.Application.AuditLogs.Queries.GetAuditLogs;

/// <summary>
/// Handles the GetAuditLogsQuery to retrieve a paginated list of audit log entries.
/// </summary>
public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PaginatedList<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AuditLogDto>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize pagination
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        // Build query
        var query = _context.Set<AuditLogEntry>().AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(a => a.EntityType == request.EntityType);
        }

        if (request.EntityId.HasValue)
        {
            query = query.Where(a => a.EntityId == request.EntityId.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(a => a.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(a => a.Action == request.Action);
        }

        if (request.From.HasValue)
        {
            query = query.Where(a => a.Timestamp >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(a => a.Timestamp <= request.To.Value);
        }

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
