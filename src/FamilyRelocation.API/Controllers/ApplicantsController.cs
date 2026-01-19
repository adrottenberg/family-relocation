using FamilyRelocation.Application.Applicants.Commands.CreateApplicant;
using FamilyRelocation.Application.Applicants.Commands.UpdateApplicant;
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
}
