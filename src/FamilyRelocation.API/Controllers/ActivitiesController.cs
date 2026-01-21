using FamilyRelocation.Application.Activities.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Controller for activity tracking and history.
/// </summary>
[ApiController]
[Route("api/activities")]
[Authorize]
public class ActivitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ActivitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets the most recent activities across all entities.
    /// </summary>
    /// <param name="count">Number of activities to return (default: 10, max: 50)</param>
    [HttpGet("recent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 10)
    {
        count = Math.Min(count, 50);
        var result = await _mediator.Send(new GetRecentActivitiesQuery(count));
        return Ok(result);
    }

    /// <summary>
    /// Gets activities for a specific entity.
    /// </summary>
    /// <param name="entityType">The type of entity (e.g., "Applicant", "Property")</param>
    /// <param name="entityId">The entity's unique identifier</param>
    [HttpGet("{entityType}/{entityId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntity(string entityType, Guid entityId)
    {
        var result = await _mediator.Send(new GetActivitiesByEntityQuery(entityType, entityId));
        return Ok(result);
    }
}
