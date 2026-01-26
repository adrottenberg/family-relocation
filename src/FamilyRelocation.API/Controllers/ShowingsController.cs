using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Showings.Commands.RescheduleShowing;
using FamilyRelocation.Application.Showings.Commands.ScheduleShowing;
using FamilyRelocation.Application.Showings.Commands.UpdateShowingStatus;
using FamilyRelocation.Application.Showings.DTOs;
using FamilyRelocation.Application.Showings.Queries.GetShowingById;
using FamilyRelocation.Application.Showings.Queries.GetShowings;
using FamilyRelocation.Application.Showings.Queries.GetUpcomingShowings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Controller for managing property showings.
/// </summary>
[ApiController]
[Route("api/showings")]
[Authorize]
public class ShowingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShowingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets showings with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ShowingListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? fromDateTime,
        [FromQuery] DateTime? toDateTime,
        [FromQuery] string? status,
        [FromQuery] Guid? brokerId,
        [FromQuery] Guid? propertyMatchId)
    {
        var result = await _mediator.Send(new GetShowingsQuery(fromDateTime, toDateTime, status, brokerId, propertyMatchId));
        return Ok(result);
    }

    /// <summary>
    /// Gets upcoming showings (today and future).
    /// </summary>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(List<ShowingListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcoming([FromQuery] int? days)
    {
        var result = await _mediator.Send(new GetUpcomingShowingsQuery(days));
        return Ok(result);
    }

    /// <summary>
    /// Gets a showing by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ShowingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetShowingByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Schedules a new showing.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Coordinator,Admin,BoardMember,Broker")]
    [ProducesResponseType(typeof(ShowingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Schedule([FromBody] ScheduleShowingRequest request)
    {
        try
        {
            var result = await _mediator.Send(new ScheduleShowingCommand(
                request.PropertyMatchId,
                request.ScheduledDateTime,
                request.Notes,
                request.BrokerUserId));
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reschedules a showing to a new date/time.
    /// </summary>
    [HttpPut("{id:guid}/reschedule")]
    [Authorize(Roles = "Coordinator,Admin,BoardMember,Broker")]
    [ProducesResponseType(typeof(ShowingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleShowingRequest request)
    {
        try
        {
            var result = await _mediator.Send(new RescheduleShowingCommand(id, request.NewScheduledDateTime));
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates showing status (complete, cancel, no-show).
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Coordinator,Admin,BoardMember,Broker")]
    [ProducesResponseType(typeof(ShowingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateShowingStatusRequest request)
    {
        try
        {
            var result = await _mediator.Send(new UpdateShowingStatusCommand(id, request.Status, request.Notes));
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
