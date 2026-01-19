using FamilyRelocation.Application.AuditLogs.DTOs;
using FamilyRelocation.Application.Common.Models;
using MediatR;

namespace FamilyRelocation.Application.AuditLogs.Queries.GetAuditLogs;

/// <summary>
/// Query to retrieve a paginated list of audit log entries with optional filters.
/// </summary>
public record GetAuditLogsQuery : IRequest<PaginatedList<AuditLogDto>>
{
    /// <summary>
    /// Filter by entity type (e.g., "Applicant", "HousingSearch").
    /// </summary>
    public string? EntityType { get; init; }

    /// <summary>
    /// Filter by specific entity ID.
    /// </summary>
    public Guid? EntityId { get; init; }

    /// <summary>
    /// Filter by user who made the change.
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Filter by action type (e.g., "Added", "Modified", "Deleted").
    /// </summary>
    public string? Action { get; init; }

    /// <summary>
    /// Filter by timestamp (on or after).
    /// </summary>
    public DateTime? From { get; init; }

    /// <summary>
    /// Filter by timestamp (on or before).
    /// </summary>
    public DateTime? To { get; init; }

    /// <summary>
    /// Page number (1-based). Default: 1
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Number of items per page. Default: 50, Max: 100
    /// </summary>
    public int PageSize { get; init; } = 50;
}
