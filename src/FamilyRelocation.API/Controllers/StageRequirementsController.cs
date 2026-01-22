using FamilyRelocation.Application.Documents.Commands.CreateStageRequirement;
using FamilyRelocation.Application.Documents.Commands.DeleteStageRequirement;
using FamilyRelocation.Application.Documents.Queries.GetAllStageRequirements;
using FamilyRelocation.Application.Documents.Queries.GetStageRequirements;
using FamilyRelocation.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Controller for managing stage transition document requirements.
/// </summary>
[ApiController]
[Route("api/stage-requirements")]
[Authorize]
public class StageRequirementsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes the controller with required dependencies.
    /// </summary>
    public StageRequirementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets all stage transition requirements.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRequirements()
    {
        var result = await _mediator.Send(new GetAllStageRequirementsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Gets document requirements for a specific stage transition.
    /// </summary>
    [HttpGet("{fromStage}/{toStage}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRequirements(
        string fromStage,
        string toStage,
        [FromQuery] Guid? applicantId = null)
    {
        if (!Enum.TryParse<HousingSearchStage>(fromStage, ignoreCase: true, out var from))
        {
            return BadRequest(new { message = $"Invalid fromStage: {fromStage}" });
        }

        if (!Enum.TryParse<HousingSearchStage>(toStage, ignoreCase: true, out var to))
        {
            return BadRequest(new { message = $"Invalid toStage: {toStage}" });
        }

        var result = await _mediator.Send(new GetStageRequirementsQuery(from, to, applicantId));
        return Ok(result);
    }

    /// <summary>
    /// Creates a new stage transition requirement. Admin only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStageRequirementRequest request)
    {
        if (!Enum.TryParse<HousingSearchStage>(request.FromStage, ignoreCase: true, out var from))
        {
            return BadRequest(new { message = $"Invalid fromStage: {request.FromStage}" });
        }

        if (!Enum.TryParse<HousingSearchStage>(request.ToStage, ignoreCase: true, out var to))
        {
            return BadRequest(new { message = $"Invalid toStage: {request.ToStage}" });
        }

        var id = await _mediator.Send(new CreateStageRequirementCommand(
            from,
            to,
            request.DocumentTypeId,
            request.IsRequired
        ));

        return CreatedAtAction(nameof(GetAllRequirements), new { id });
    }

    /// <summary>
    /// Deletes a stage transition requirement. Admin only.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteStageRequirementCommand(id));

        if (!success)
            return NotFound();

        return NoContent();
    }
}

/// <summary>
/// Request body for creating a stage requirement.
/// </summary>
public record CreateStageRequirementRequest(
    string FromStage,
    string ToStage,
    Guid DocumentTypeId,
    bool IsRequired
);
