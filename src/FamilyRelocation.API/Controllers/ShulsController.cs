using FamilyRelocation.Application.Shuls.Commands.CreateShul;
using FamilyRelocation.Application.Shuls.Commands.DeleteShul;
using FamilyRelocation.Application.Shuls.Commands.UpdateShul;
using FamilyRelocation.Application.Shuls.Queries.GetShulById;
using FamilyRelocation.Application.Shuls.Queries.GetShuls;
using FamilyRelocation.Application.Shuls.Queries.GetPropertyShulDistances;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Controller for managing shuls (synagogues).
/// </summary>
[ApiController]
[Route("api/shuls")]
[Authorize]
public class ShulsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShulsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a paginated list of shuls with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetShulsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets a shul by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetShulByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Creates a new shul.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Coordinator,Admin,BoardMember,Broker")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateShulCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing shul.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Coordinator,Admin,BoardMember,Broker")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShulCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "ID in URL does not match ID in request body" });
        }

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes (deactivates) a shul.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Coordinator,Admin,BoardMember,Broker")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteShulCommand(id));
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets walking distances from a property to all shuls.
    /// </summary>
    [HttpGet("distances/property/{propertyId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPropertyDistances(Guid propertyId)
    {
        var result = await _mediator.Send(new GetPropertyShulDistancesQuery(propertyId));
        return Ok(result);
    }
}
