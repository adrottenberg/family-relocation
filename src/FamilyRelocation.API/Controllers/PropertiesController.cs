using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Properties.Commands.AddPropertyPhoto;
using FamilyRelocation.Application.Properties.Commands.CreateProperty;
using FamilyRelocation.Application.Properties.Commands.DeleteProperty;
using FamilyRelocation.Application.Properties.Commands.DeletePropertyPhoto;
using FamilyRelocation.Application.Properties.Commands.SetPrimaryPhoto;
using FamilyRelocation.Application.Properties.Commands.UpdateProperty;
using FamilyRelocation.Application.Properties.Commands.UpdatePropertyStatus;
using FamilyRelocation.Application.Properties.Queries.GetProperties;
using FamilyRelocation.Application.Properties.Queries.GetPropertyById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Controller for managing property listings.
/// </summary>
[ApiController]
[Route("api/properties")]
[Authorize]
public class PropertiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a paginated list of properties with search, filter, and sort options.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetPropertiesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets a property by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPropertyByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Creates a new property listing.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Coordinator,Admin,Broker")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePropertyCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing property.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Coordinator,Admin,Broker")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePropertyCommand command)
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
    }

    /// <summary>
    /// Updates a property's listing status.
    /// </summary>
    /// <remarks>
    /// Valid statuses: Active, UnderContract, Sold, OffMarket
    /// </remarks>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Coordinator,Admin,Broker")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var result = await _mediator.Send(new UpdatePropertyStatusCommand(id, request.Status));
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
    /// Soft deletes a property.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Coordinator,Admin,Broker")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeletePropertyCommand(id));
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    /// <summary>
    /// Uploads a photo for a property.
    /// </summary>
    /// <remarks>
    /// Maximum 10 photos per property. Accepted formats: JPEG, PNG.
    /// </remarks>
    [HttpPost("{id:guid}/photos")]
    [Authorize(Roles = "Coordinator,Admin,Broker")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file, [FromForm] string? description = null)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/png" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new { message = "Only JPEG and PNG images are allowed" });
        }

        // Validate file size (max 10MB)
        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest(new { message = "File size cannot exceed 10MB" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var photoId = await _mediator.Send(new AddPropertyPhotoCommand(
                id,
                stream,
                file.FileName,
                file.ContentType,
                description));

            return CreatedAtAction(nameof(GetById), new { id }, new { photoId });
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
    /// Deletes a photo from a property.
    /// </summary>
    [HttpDelete("{id:guid}/photos/{photoId:guid}")]
    [Authorize(Roles = "Coordinator,Admin,Broker")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePhoto(Guid id, Guid photoId)
    {
        try
        {
            await _mediator.Send(new DeletePropertyPhotoCommand(id, photoId));
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Sets a photo as the primary photo for a property.
    /// </summary>
    [HttpPut("{id:guid}/photos/{photoId:guid}/primary")]
    [Authorize(Roles = "Coordinator,Admin,Broker")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPrimaryPhoto(Guid id, Guid photoId)
    {
        try
        {
            await _mediator.Send(new SetPrimaryPhotoCommand(id, photoId));
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public record UpdateStatusRequest(string Status);
