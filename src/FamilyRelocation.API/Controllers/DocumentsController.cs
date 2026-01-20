using FamilyRelocation.Application.Common.Exceptions;
using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Application.Documents.Commands.DeleteDocument;
using FamilyRelocation.Application.Documents.Commands.UploadDocument;
using FamilyRelocation.Application.Documents.Queries.GetApplicantDocuments;
using FamilyRelocation.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyRelocation.API.Controllers;

/// <summary>
/// Controller for document upload and retrieval operations.
/// </summary>
[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentStorageService _storageService;
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    private static readonly string[] AllowedContentTypes =
        ["application/pdf", "image/jpeg", "image/png"];
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public DocumentsController(
        IDocumentStorageService storageService,
        IApplicationDbContext context,
        IMediator mediator)
    {
        _storageService = storageService;
        _context = context;
        _mediator = mediator;
    }

    /// <summary>
    /// Gets all documents for an applicant.
    /// </summary>
    [HttpGet("applicant/{applicantId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApplicantDocuments(Guid applicantId)
    {
        var result = await _mediator.Send(new GetApplicantDocumentsQuery(applicantId));
        return Ok(result);
    }

    /// <summary>
    /// Uploads a document to cloud storage and records it in the database.
    /// </summary>
    /// <remarks>
    /// Supported file types: PDF, JPEG, PNG
    /// Maximum file size: 10MB
    ///
    /// The documentTypeId must be a valid, active document type ID.
    /// Use GET /api/document-types to get available document types.
    /// </remarks>
    [HttpPost("upload")]
    [RequestSizeLimit(MaxFileSize)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] Guid applicantId,
        [FromForm] Guid documentTypeId,
        CancellationToken cancellationToken)
    {
        // Validate file
        if (file.Length == 0)
        {
            return BadRequest(new { message = "File is empty" });
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(new { message = "File exceeds 10MB limit" });
        }

        if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new { message = "Only PDF and image files (JPEG, PNG) are allowed" });
        }

        // Get document type for naming
        var documentType = await _context.Set<DocumentType>()
            .FirstOrDefaultAsync(dt => dt.Id == documentTypeId, cancellationToken);

        if (documentType == null)
        {
            return NotFound(new { message = "Document type not found" });
        }

        if (!documentType.IsActive)
        {
            return BadRequest(new { message = "Document type is not active" });
        }

        // Validate applicant exists and get family name for file naming
        var applicant = await _context.Set<Applicant>()
            .FirstOrDefaultAsync(a => a.Id == applicantId, cancellationToken);

        if (applicant == null)
        {
            return NotFound(new { message = "Applicant not found" });
        }

        // Generate storage key: {DocumentType}_{FamilyName}_{yyyyMMdd_HHmmss}.{ext}
        // Example: BrokerAgreement_Goldstein_20260120_143052.pdf
        var storageKey = GenerateStorageKey(documentType.Name, applicant.FamilyName, file.FileName);

        // Upload to storage
        await using var stream = file.OpenReadStream();
        var result = await _storageService.UploadAsync(
            stream,
            storageKey,
            file.ContentType,
            cancellationToken);

        try
        {
            // Record the document in the database
            var command = new UploadDocumentCommand(
                applicantId,
                documentTypeId,
                file.FileName,
                result.StorageKey,
                file.ContentType,
                file.Length);

            var documentRecord = await _mediator.Send(command, cancellationToken);

            return Ok(new
            {
                document = documentRecord,
                uploadResult = result
            });
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
    /// Deletes a document from the database.
    /// </summary>
    /// <remarks>
    /// Note: This removes the database record. The file in S3 may need separate cleanup.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteDocumentCommand(id));
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a pre-signed URL for temporary access to a document.
    /// </summary>
    /// <remarks>
    /// The URL is valid for 1 hour by default.
    /// Use this to view or download documents stored in S3.
    /// </remarks>
    [HttpGet("presigned-url")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPresignedUrl(
        [FromQuery] string documentKey,
        [FromQuery] int? expiryMinutes,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentKey))
        {
            return BadRequest(new { message = "Document key is required" });
        }

        var expiry = TimeSpan.FromMinutes(expiryMinutes ?? 60);
        if (expiry.TotalMinutes > 1440) // 24 hours max
        {
            return BadRequest(new { message = "Expiry cannot exceed 24 hours" });
        }

        var url = await _storageService.GetPreSignedUrlAsync(documentKey, expiry, cancellationToken);

        return Ok(new { url, expiresAt = DateTime.UtcNow.Add(expiry) });
    }

    /// <summary>
    /// Generates a storage key using the naming convention: {DocumentType}_{FamilyName}_{yyyyMMdd_HHmmss}.{ext}
    /// </summary>
    private static string GenerateStorageKey(string documentType, string familyName, string originalFileName)
    {
        var safeDocumentType = SanitizeName(documentType);
        var safeFamilyName = SanitizeName(familyName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var extension = Path.GetExtension(originalFileName).TrimStart('.');

        return $"{safeDocumentType}_{safeFamilyName}_{timestamp}.{extension}";
    }

    /// <summary>
    /// Sanitizes a name for use in storage keys.
    /// Removes invalid characters and spaces.
    /// </summary>
    private static string SanitizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Unknown";

        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Replace(" ", "");
    }
}
