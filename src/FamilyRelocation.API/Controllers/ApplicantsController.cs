using FamilyRelocation.Application.Applicants.Commands.ChangeStage;
using FamilyRelocation.Application.Applicants.Commands.CreateApplicant;
using FamilyRelocation.Application.Applicants.Commands.DeleteApplicant;
using FamilyRelocation.Application.Applicants.Commands.RecordAgreement;
using FamilyRelocation.Application.Applicants.Commands.SetBoardDecision;
using FamilyRelocation.Application.Applicants.Commands.UpdateApplicant;
using FamilyRelocation.Application.Applicants.Commands.UpdatePreferences;
using FamilyRelocation.Application.Applicants.DTOs;
using FamilyRelocation.Application.Applicants.Queries.GetApplicantById;
using FamilyRelocation.Application.Applicants.Queries.GetApplicants;
using FamilyRelocation.Application.Common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Controller for managing applicant family records.
/// </summary>
[ApiController]
[Route("api/applicants")]
[Authorize]
public class ApplicantsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes the controller with required dependencies.
    /// </summary>
    /// <param name="mediator">MediatR mediator for CQRS.</param>
    public ApplicantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a paginated list of applicants with search, filter, and sort options.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetApplicantsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new applicant (family) and their housing search.
    /// Publicly accessible for board approval applications.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateApplicantCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.ApplicantId }, result);
        }
        catch (DuplicateEmailException ex)
        {
            return Conflict(new { message = ex.Message, email = ex.Email });
        }
    }

    /// <summary>
    /// Gets an applicant by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetApplicantByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Updates an existing applicant's basic information.
    /// Cannot update: board decision, created date, applicant ID.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApplicantCommand command)
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
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (DuplicateEmailException ex)
        {
            return Conflict(new { message = ex.Message, email = ex.Email });
        }
    }

    /// <summary>
    /// Soft deletes an applicant.
    /// The applicant will no longer appear in list or pipeline queries.
    /// </summary>
    /// <remarks>
    /// This is a soft delete - the applicant record remains in the database
    /// with IsDeleted = true. The applicant can be restored if needed.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteApplicantCommand(id));
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Changes the housing search stage for an applicant.
    /// Required fields depend on the target stage.
    /// </summary>
    /// <remarks>
    /// Stage transitions and required fields:
    /// - HouseHunting: Board approval AND signed agreements required (from Submitted), reason optional (from UnderContract/Closed if contract fell through)
    /// - Rejected: Reason optional
    /// - Paused: Reason optional
    /// - UnderContract: Contract details required (price, optional propertyId and expectedClosingDate)
    /// - Closed: ClosingDate required
    /// - MovedIn: MovedInDate required
    ///
    /// Note: Both broker agreement and community takanos must be signed before starting house hunting.
    /// Use POST /api/applicants/{id}/agreements to record signed agreements.
    /// </remarks>
    [HttpPut("{id:guid}/stage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStage(Guid id, [FromBody] ChangeStageRequest request)
    {
        try
        {
            var command = new ChangeStageCommand(id, request);
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
    /// Records that an applicant has signed a required agreement.
    /// Both broker agreement and community takanos must be signed before starting house hunting.
    /// </summary>
    /// <remarks>
    /// Agreement types:
    /// - BrokerAgreement: Agreement to work with our broker
    /// - CommunityTakanos: Community guidelines agreement
    ///
    /// Upload the signed document first, then call this endpoint with the document URL.
    /// </remarks>
    [HttpPost("{id:guid}/agreements")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordAgreement(Guid id, [FromBody] RecordAgreementRequest request)
    {
        try
        {
            var command = new RecordAgreementCommand(id, request);
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the housing preferences for an applicant's housing search.
    /// </summary>
    /// <remarks>
    /// Updates housing search criteria including:
    /// - Budget amount
    /// - Minimum bedrooms/bathrooms
    /// - Required features (e.g., basement, garage, yard)
    /// - Shul proximity preferences
    /// - Move timeline
    /// </remarks>
    [HttpPut("{id:guid}/preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePreferences(Guid id, [FromBody] HousingPreferencesDto request)
    {
        try
        {
            var command = new UpdatePreferencesCommand(id, request);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Sets the board's decision on an applicant and transitions the stage accordingly.
    /// </summary>
    /// <remarks>
    /// Records the board's decision and automatically transitions the housing search stage:
    /// - Approved: Transitions to BoardApproved stage. Applicant can then sign agreements and start house hunting.
    /// - Rejected: Transitions to Rejected stage.
    /// - Deferred: No stage change. Applicant remains in Submitted stage for future review.
    /// - Pending: No stage change. Resets decision to pending.
    ///
    /// Can only set decision when applicant is in Submitted stage.
    /// The 'notes' field can be used for approval notes, rejection reason, or deferral reason.
    /// </remarks>
    [HttpPut("{id:guid}/board-review")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetBoardDecision(Guid id, [FromBody] SetBoardDecisionRequest request)
    {
        try
        {
            var command = new SetBoardDecisionCommand(id, request);
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
}
