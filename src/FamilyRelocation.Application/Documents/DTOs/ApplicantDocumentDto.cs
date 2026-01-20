namespace FamilyRelocation.Application.Documents.DTOs;

/// <summary>
/// DTO for applicant document information.
/// </summary>
public record ApplicantDocumentDto
{
    /// <summary>
    /// Unique identifier for the document.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The document type ID.
    /// </summary>
    public required Guid DocumentTypeId { get; init; }

    /// <summary>
    /// Display name of the document type.
    /// </summary>
    public required string DocumentTypeName { get; init; }

    /// <summary>
    /// Original filename as uploaded by the user.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// S3 storage key (path in bucket).
    /// </summary>
    public required string StorageKey { get; init; }

    /// <summary>
    /// MIME content type (e.g., application/pdf).
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public required long FileSizeBytes { get; init; }

    /// <summary>
    /// When the document was uploaded.
    /// </summary>
    public required DateTime UploadedAt { get; init; }

    /// <summary>
    /// User ID who uploaded the document.
    /// </summary>
    public Guid? UploadedBy { get; init; }
}
