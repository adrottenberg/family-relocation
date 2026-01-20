using FamilyRelocation.Domain.Common;

namespace FamilyRelocation.Domain.Entities;

/// <summary>
/// A document uploaded for an applicant (e.g., signed agreements, supporting documents).
/// Documents are linked to both an Applicant and a DocumentType.
/// </summary>
public class ApplicantDocument : Entity<Guid>
{
    /// <summary>
    /// The applicant this document belongs to
    /// </summary>
    public Guid ApplicantId { get; private set; }

    /// <summary>
    /// The type of document (e.g., BrokerAgreement, CommunityTakanos)
    /// </summary>
    public Guid DocumentTypeId { get; private set; }

    /// <summary>
    /// Original filename as uploaded by the user
    /// </summary>
    public string FileName { get; private set; } = null!;

    /// <summary>
    /// S3 storage key (path in bucket).
    /// Format: {DocumentType}_{FamilyName}_{yyyyMMdd_HHmmss}.{ext}
    /// </summary>
    public string StorageKey { get; private set; } = null!;

    /// <summary>
    /// MIME content type (e.g., application/pdf, image/jpeg)
    /// </summary>
    public string ContentType { get; private set; } = null!;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>
    /// When the document was uploaded
    /// </summary>
    public DateTime UploadedAt { get; private set; }

    /// <summary>
    /// User ID who uploaded the document (null if system/migration)
    /// </summary>
    public Guid? UploadedBy { get; private set; }

    // Navigation properties
    public virtual DocumentType DocumentType { get; private set; } = null!;
    public virtual Applicant Applicant { get; private set; } = null!;

    private ApplicantDocument() { }

    /// <summary>
    /// Factory method to create a new applicant document
    /// </summary>
    public static ApplicantDocument Create(
        Guid applicantId,
        Guid documentTypeId,
        string fileName,
        string storageKey,
        string contentType,
        long fileSizeBytes,
        Guid? uploadedBy = null)
    {
        if (applicantId == Guid.Empty)
            throw new ArgumentException("Applicant ID is required", nameof(applicantId));

        if (documentTypeId == Guid.Empty)
            throw new ArgumentException("Document type ID is required", nameof(documentTypeId));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required", nameof(fileName));

        if (string.IsNullOrWhiteSpace(storageKey))
            throw new ArgumentException("Storage key is required", nameof(storageKey));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type is required", nameof(contentType));

        if (fileSizeBytes <= 0)
            throw new ArgumentException("File size must be positive", nameof(fileSizeBytes));

        return new ApplicantDocument
        {
            Id = Guid.NewGuid(),
            ApplicantId = applicantId,
            DocumentTypeId = documentTypeId,
            FileName = fileName.Trim(),
            StorageKey = storageKey.Trim(),
            ContentType = contentType.Trim(),
            FileSizeBytes = fileSizeBytes,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy
        };
    }

    /// <summary>
    /// Update the storage key (used when replacing a document)
    /// </summary>
    public void UpdateStorage(string newStorageKey, string newFileName, string contentType, long fileSizeBytes, Guid? uploadedBy)
    {
        if (string.IsNullOrWhiteSpace(newStorageKey))
            throw new ArgumentException("Storage key is required", nameof(newStorageKey));

        StorageKey = newStorageKey.Trim();
        FileName = newFileName.Trim();
        ContentType = contentType.Trim();
        FileSizeBytes = fileSizeBytes;
        UploadedAt = DateTime.UtcNow;
        UploadedBy = uploadedBy;
    }
}
