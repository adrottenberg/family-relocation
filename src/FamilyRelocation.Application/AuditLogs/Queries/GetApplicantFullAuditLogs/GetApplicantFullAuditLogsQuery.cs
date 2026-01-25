using FamilyRelocation.Application.AuditLogs.DTOs;
using FamilyRelocation.Application.Common.Models;
using MediatR;

namespace FamilyRelocation.Application.AuditLogs.Queries.GetApplicantFullAuditLogs;

/// <summary>
/// Query to get audit logs for an applicant including their housing search history.
/// </summary>
public record GetApplicantFullAuditLogsQuery : IRequest<PaginatedList<AuditLogDto>>
{
    public required Guid ApplicantId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
