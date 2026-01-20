using FamilyRelocation.Application.Common.Interfaces;
using FamilyRelocation.Domain.Entities;
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

    private static readonly string[] AllowedContentTypes =
        ["application/pdf", "image/jpeg", "image/png"];
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public DocumentsController(
        IDocumentStorageService storageService,
        IApplicationDbContext context)
    {
        _storageService = storageService;
        _context = context;
    }

    /// <summary>
    /// Uploads a document to cloud storage.
    /// </summary>
    /// <remarks>
    /// Supported file types: PDF, JPEG, PNG
    /// Maximum file size: 10MB
    ///
    /// Document types:
    /// - BrokerAgreement: Signed broker agreement
    /// - CommunityTakanos: Signed community guidelines
    /// - Other: Other supporting documents
    /// </remarks>
    [HttpPost("upload")]
    [RequestSizeLimit(MaxFileSize)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] Guid applicantId,
        [FromForm] string documentType,
        CancellationToken cancellationToken)
    {
        // Validate applicant exists
        var applicantExists = await _context.Set<Applicant>()
            .AnyAsync(a => a.Id == applicantId, cancellationToken);

        if (!applicantExists)
        {
            return NotFound(new { message = "Applicant not found" });
        }

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

        // Validate document type
        var validTypes = new[] { "BrokerAgreement", "CommunityTakanos", "Other" };
        if (!validTypes.Contains(documentType, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = $"Invalid document type. Valid types: {string.Join(", ", validTypes)}"
            });
        }

        // Upload to S3
        await using var stream = file.OpenReadStream();
        var result = await _storageService.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            applicantId,
            documentType,
            cancellationToken);

        return Ok(result);
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
}
