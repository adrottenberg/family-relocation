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
    /// <param name="mediator">MediatR mediator for CQRS.</param>
    public StageRequirementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets document requirements for a specific stage transition.
    /// </summary>
    /// <param name="fromStage">The stage transitioning from.</param>
    /// <param name="toStage">The stage transitioning to.</param>
    /// <param name="applicantId">Optional applicant ID to check which documents are already uploaded.</param>
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
}
