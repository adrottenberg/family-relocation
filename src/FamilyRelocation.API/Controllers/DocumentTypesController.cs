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
    /// <param name="mediator">MediatR mediator for CQRS.</param>
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
}
