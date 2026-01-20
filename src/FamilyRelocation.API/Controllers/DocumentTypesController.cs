using FamilyRelocation.Application.Documents.Commands.CreateDocumentType;
using FamilyRelocation.Application.Documents.Commands.DeleteDocumentType;
using FamilyRelocation.Application.Documents.Commands.UpdateDocumentType;
using FamilyRelocation.Application.Documents.Queries.GetDocumentTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Controller for managing document types.
/// </summary>
[ApiController]
[Route("api/document-types")]
[Authorize]
public class DocumentTypesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes the controller with required dependencies.
    /// </summary>
    public DocumentTypesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets all document types.
    /// </summary>
    /// <param name="activeOnly">If true (default), only return active document types.</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var result = await _mediator.Send(new GetDocumentTypesQuery(activeOnly));
        return Ok(result);
    }

    /// <summary>
    /// Creates a new document type.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDocumentTypeRequest request)
    {
        var id = await _mediator.Send(new CreateDocumentTypeCommand(
            request.Name,
            request.DisplayName,
            request.Description
        ));

        return CreatedAtAction(nameof(GetAll), new { id });
    }

    /// <summary>
    /// Updates an existing document type.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDocumentTypeRequest request)
    {
        var success = await _mediator.Send(new UpdateDocumentTypeCommand(
            id,
            request.DisplayName,
            request.Description
        ));

        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Deactivates (soft deletes) a document type.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var success = await _mediator.Send(new DeleteDocumentTypeCommand(id));

            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

/// <summary>
/// Request body for creating a document type.
/// </summary>
public record CreateDocumentTypeRequest(string Name, string DisplayName, string? Description);

/// <summary>
/// Request body for updating a document type.
/// </summary>
public record UpdateDocumentTypeRequest(string DisplayName, string? Description);
