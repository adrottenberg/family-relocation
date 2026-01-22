using FamilyRelocation.Application.Activities.Commands.LogActivity;
using FamilyRelocation.Application.Activities.Queries;
using MediatR;
using ActivityDto = FamilyRelocation.Application.Activities.Queries.ActivityDto;
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
    /// Gets a single activity by ID.
    /// </summary>
    /// <param name="id">The activity's unique identifier</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetActivityByIdQuery(id));
        if (result == null)
            return NotFound(new { message = "Activity not found" });
        return Ok(result);
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
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    [HttpGet("{entityType}/{entityId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntity(
        string entityType,
        Guid entityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Min(pageSize, 100);
        page = Math.Max(page, 1);
        var result = await _mediator.Send(new GetActivitiesByEntityQuery(entityType, entityId, page, pageSize));
        return Ok(result);
    }

    /// <summary>
    /// Logs a manual activity (phone call, note, etc.) for an entity.
    /// </summary>
    /// <param name="request">The activity details</param>
    [HttpPost]
    [ProducesResponseType(typeof(LogActivityResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogActivity([FromBody] LogActivityRequest request)
    {
        var command = new LogActivityCommand(
            EntityType: request.EntityType,
            EntityId: request.EntityId,
            Type: request.Type,
            Description: request.Description,
            DurationMinutes: request.DurationMinutes,
            Outcome: request.Outcome,
            CreateFollowUp: request.CreateFollowUp,
            FollowUpDate: request.FollowUpDate,
            FollowUpTitle: request.FollowUpTitle
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(LogActivity), new { id = result.ActivityId }, result);
    }
}

/// <summary>
/// Request body for logging a manual activity.
/// </summary>
public record LogActivityRequest(
    /// <summary>
    /// The type of entity (Applicant, Property, HousingSearch).
    /// </summary>
    string EntityType,

    /// <summary>
    /// The entity's unique identifier.
    /// </summary>
    Guid EntityId,

    /// <summary>
    /// The activity type (PhoneCall, Note, Email, SMS).
    /// </summary>
    string Type,

    /// <summary>
    /// Description or notes about the activity.
    /// </summary>
    string Description,

    /// <summary>
    /// Duration in minutes (for phone calls).
    /// </summary>
    int? DurationMinutes = null,

    /// <summary>
    /// Call outcome (Connected, Voicemail, NoAnswer, Busy, LeftMessage).
    /// </summary>
    string? Outcome = null,

    /// <summary>
    /// Whether to create a follow-up reminder.
    /// </summary>
    bool CreateFollowUp = false,

    /// <summary>
    /// Date for the follow-up reminder.
    /// </summary>
    DateTime? FollowUpDate = null,

    /// <summary>
    /// Title for the follow-up reminder.
    /// </summary>
    string? FollowUpTitle = null
);
