using FamilyRelocation.Application.AuditLogs.Queries.GetAuditLogs;
using FamilyRelocation.Application.AuditLogs.Queries.GetApplicantFullAuditLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Controller for querying audit log entries.
/// </summary>
[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Coordinator,Admin,BoardMember,Broker")]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes the controller with required dependencies.
    /// </summary>
    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a paginated list of audit log entries with optional filters.
    /// </summary>
    /// <param name="entityType">Filter by entity type (e.g., "Applicant", "HousingSearch").</param>
    /// <param name="entityId">Filter by specific entity ID.</param>
    /// <param name="userId">Filter by user who made the change.</param>
    /// <param name="action">Filter by action type (Added, Modified, Deleted).</param>
    /// <param name="from">Filter by timestamp (on or after).</param>
    /// <param name="to">Filter by timestamp (on or before).</param>
    /// <param name="page">Page number (1-based). Default: 1</param>
    /// <param name="pageSize">Items per page. Default: 50, Max: 100</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetAuditLogsQuery
        {
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            Action = action,
            From = from,
            To = to,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets audit history for a specific applicant including their housing search history.
    /// </summary>
    /// <param name="applicantId">The applicant ID.</param>
    /// <param name="page">Page number (1-based). Default: 1</param>
    /// <param name="pageSize">Items per page. Default: 50, Max: 100</param>
    [HttpGet("applicant/{applicantId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByApplicant(
        Guid applicantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetApplicantFullAuditLogsQuery
        {
            ApplicantId = applicantId,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
