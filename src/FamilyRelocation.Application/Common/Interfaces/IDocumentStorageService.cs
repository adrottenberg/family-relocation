namespace FamilyRelocation.Application.Common.Interfaces;

/// <summary>
/// Service for storing and retrieving documents from cloud storage.
/// </summary>
public interface IDocumentStorageService
{
    /// <summary>
    /// Uploads a document to storage.
    /// </summary>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="fileName">Original file name.</param>
    /// <param name="contentType">MIME type of the file.</param>
    /// <param name="applicantId">Associated applicant ID.</param>
    /// <param name="documentType">Type of document (e.g., BrokerAgreement, CommunityTakanos).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Upload result with URL and metadata.</returns>
    Task<DocumentUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Guid applicantId,
        string documentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pre-signed URL for temporary access to a document.
    /// </summary>
    /// <param name="documentKey">The storage key/path of the document.</param>
    /// <param name="expiry">How long the URL should be valid.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Pre-signed URL for accessing the document.</returns>
    Task<string> GetPreSignedUrlAsync(
        string documentKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a document upload operation.
/// </summary>
public record DocumentUploadResult(
    string DocumentUrl,
    string DocumentKey,
    string FileName,
    long FileSize,
    string ContentType,
    DateTime UploadedAt);
