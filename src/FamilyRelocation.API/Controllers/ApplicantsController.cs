using FamilyRelocation.Application.Applicants.Commands.CreateApplicant;
using FamilyRelocation.Application.Applicants.Commands.DeleteApplicant;
using FamilyRelocation.Application.Applicants.Commands.SetBoardDecision;
using FamilyRelocation.Application.Applicants.Commands.UpdateApplicant;
using FamilyRelocation.Application.Applicants.Queries.GetApplicantById;
using FamilyRelocation.Application.Applicants.Queries.GetApplicants;
using FamilyRelocation.Application.Common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    [Authorize(Roles = "Coordinator,Admin,BoardMember,Broker")]
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
    [EnableRateLimiting("public-form")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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
    /// Gets an applicant by ID with full details including PII.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Coordinator,Admin,BoardMember,Broker")]
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
    [Authorize(Roles = "Coordinator,Admin,BoardMember,Broker")]
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
    [Authorize(Roles = "Coordinator,Admin,BoardMember")]
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
    /// Sets the board's decision on an applicant and updates status accordingly.
    /// </summary>
    /// <remarks>
    /// Records the board's decision and automatically updates the applicant:
    /// - Approved: Sets ApplicationStatus to Approved and creates a HousingSearch in AwaitingAgreements stage.
    ///   Returns the new housingSearchId. Applicant can now proceed through the pipeline.
    /// - Rejected: Sets ApplicationStatus to Rejected. No HousingSearch is created. Terminal state.
    /// - Deferred: No status change. Applicant remains in Submitted status for future review.
    /// - Pending: No status change. Resets decision to pending.
    ///
    /// Can only set decision when applicant has Submitted status.
    /// The 'notes' field can be used for approval notes, rejection reason, or deferral reason.
    /// </remarks>
    [HttpPut("{id:guid}/board-review")]
    [Authorize(Roles = "BoardMember,Admin")]
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
