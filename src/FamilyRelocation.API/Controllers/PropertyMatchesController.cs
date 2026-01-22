using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.PropertyMatches.Commands.CreatePropertyMatch;
using FamilyRelocation.Application.PropertyMatches.Commands.DeletePropertyMatch;
using FamilyRelocation.Application.PropertyMatches.Commands.RequestShowing;
using FamilyRelocation.Application.PropertyMatches.Commands.UpdatePropertyMatchStatus;
using FamilyRelocation.Application.PropertyMatches.DTOs;
using FamilyRelocation.Application.PropertyMatches.Queries.GetPropertyMatchById;
using FamilyRelocation.Application.PropertyMatches.Queries.GetPropertyMatchesForHousingSearch;
using FamilyRelocation.Application.PropertyMatches.Queries.GetPropertyMatchesForProperty;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Controller for managing property matches between housing searches and properties.
/// </summary>
[ApiController]
[Route("api/property-matches")]
[Authorize]
public class PropertyMatchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertyMatchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets property matches for a housing search or property.
    /// </summary>
    /// <remarks>
    /// Provide either housingSearchId or propertyId (not both).
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(List<PropertyMatchListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? housingSearchId,
        [FromQuery] Guid? propertyId,
        [FromQuery] string? status)
    {
        if (housingSearchId.HasValue && propertyId.HasValue)
        {
            return BadRequest(new { message = "Provide either housingSearchId or propertyId, not both" });
        }

        if (!housingSearchId.HasValue && !propertyId.HasValue)
        {
            return BadRequest(new { message = "Either housingSearchId or propertyId is required" });
        }

        List<PropertyMatchListDto> result;

        if (housingSearchId.HasValue)
        {
            result = await _mediator.Send(new GetPropertyMatchesForHousingSearchQuery(housingSearchId.Value, status));
        }
        else
        {
            result = await _mediator.Send(new GetPropertyMatchesForPropertyQuery(propertyId!.Value, status));
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets a property match by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PropertyMatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPropertyMatchByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Creates a new property match (manual matching).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Coordinator,Admin")]
    [ProducesResponseType(typeof(PropertyMatchDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreatePropertyMatchRequest request)
    {
        try
        {
            var result = await _mediator.Send(new CreatePropertyMatchCommand(
                request.HousingSearchId,
                request.PropertyId,
                request.Notes));
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates a property match status.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Coordinator,Admin")]
    [ProducesResponseType(typeof(PropertyMatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdatePropertyMatchStatusRequest request)
    {
        try
        {
            var result = await _mediator.Send(new UpdatePropertyMatchStatusCommand(id, request.Status, request.Notes));
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
    }

    /// <summary>
    /// Requests showings for one or more property matches (batch operation).
    /// </summary>
    [HttpPost("request-showings")]
    [Authorize(Roles = "Coordinator,Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestShowings([FromBody] List<Guid> matchIds)
    {
        var count = await _mediator.Send(new RequestShowingCommand(matchIds));
        return Ok(new { updatedCount = count });
    }

    /// <summary>
    /// Deletes a property match.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Coordinator,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeletePropertyMatchCommand(id));
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
