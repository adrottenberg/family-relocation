namespace FamilyRelocation.Application.Common.Interfaces;

/// <summary>
/// Service for storing and retrieving documents from cloud storage.
/// </summary>
public interface IDocumentStorageService
{
    /// <summary>
    /// Uploads a document to storage.
    /// Storage key format: {DocumentType}_{FamilyName}_{yyyyMMdd_HHmmss}.{ext}
    /// Example: BrokerAgreement_Goldstein_20260120_143052.pdf
    /// </summary>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="fileName">Original file name (used for extension).</param>
    /// <param name="contentType">MIME type of the file.</param>
    /// <param name="documentType">Type of document (e.g., BrokerAgreement, CommunityTakanos).</param>
    /// <param name="familyName">Family name for the storage key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Upload result with URL and metadata.</returns>
    Task<DocumentUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string documentType,
        string familyName,
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
