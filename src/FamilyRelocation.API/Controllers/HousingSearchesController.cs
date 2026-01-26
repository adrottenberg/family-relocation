using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.HousingSearches.Commands.ChangeStage;
using FamilyRelocation.Application.HousingSearches.Commands.DeactivateHousingSearch;
using FamilyRelocation.Application.HousingSearches.Commands.UpdatePreferences;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Controller for managing housing searches.
/// Housing searches are created automatically when an applicant is approved by the board.
/// </summary>
[ApiController]
[Route("api/housing-searches")]
[Authorize]
public class HousingSearchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public HousingSearchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Changes the stage of a housing search.
    /// Required fields depend on the target stage.
    /// </summary>
    /// <remarks>
    /// Stage transitions and required fields:
    /// - AwaitingAgreements -> Searching: Requires agreements to be signed (checked by document requirements)
    /// - Searching -> UnderContract: Contract details required (price, optional propertyId and expectedClosingDate)
    /// - Searching -> Paused: Reason optional
    /// - UnderContract -> Closed: ClosingDate required
    /// - UnderContract -> Searching: Contract fell through, reason optional
    /// - Closed -> MovedIn: MovedInDate required
    /// - Closed -> Searching: Contract fell through after closing, reason optional
    /// - Paused -> Searching: Resume house hunting
    ///
    /// Use GET /api/stage-requirements/{fromStage}/{toStage} to check required documents for transitions.
    /// </remarks>
    [HttpPut("{id:guid}/stage")]
    [Authorize(Roles = "Coordinator,Admin,BoardMember,Broker")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStage(Guid id, [FromBody] ChangeHousingSearchStageRequest request)
    {
        try
        {
            var command = new ChangeHousingSearchStageCommand(id, request);
            var result = await _mediator.Send(command);
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

    /// <summary>
    /// Updates the housing preferences for a housing search.
    /// </summary>
    /// <remarks>
    /// Housing preferences can be updated at any time during the search.
    /// Initial preferences are copied from the applicant when the housing search is created.
    /// </remarks>
    [HttpPut("{id:guid}/preferences")]
    [Authorize(Roles = "Coordinator,Admin,BoardMember,Broker")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePreferences(Guid id, [FromBody] UpdateHousingSearchPreferencesRequest request)
    {
        try
        {
            var command = new UpdateHousingSearchPreferencesCommand(id, request);
            var result = await _mediator.Send(command);
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
    /// Deactivates a housing search.
    /// </summary>
    /// <remarks>
    /// Used to clean up duplicate or orphaned housing searches.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        try
        {
            await _mediator.Send(new DeactivateHousingSearchCommand(id));
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
