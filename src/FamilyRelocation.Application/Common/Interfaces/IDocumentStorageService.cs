namespace FamilyRelocation.Application.Common.Interfaces;

/// <summary>
/// Service for storing and retrieving documents from cloud storage.
/// This is a simple storage abstraction - naming conventions are handled by the caller.
/// </summary>
public interface IDocumentStorageService
{
    /// <summary>
    /// Uploads a document to storage with the specified key.
    /// </summary>
    /// <param name="fileStream">The file content stream.</param>
    /// <param name="storageKey">The storage key/path for the document (caller determines naming).</param>
    /// <param name="contentType">MIME type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Upload result with URL and metadata.</returns>
    Task<DocumentUploadResult> UploadAsync(
        Stream fileStream,
        string storageKey,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pre-signed URL for temporary access to a document.
    /// </summary>
    /// <param name="storageKey">The storage key/path of the document.</param>
    /// <param name="expiry">How long the URL should be valid.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Pre-signed URL for accessing the document.</returns>
    Task<string> GetPreSignedUrlAsync(
        string storageKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a document upload operation.
/// </summary>
public record DocumentUploadResult(
    string StorageUrl,
    string StorageKey,
    long FileSize,
    string ContentType,
    DateTime UploadedAt);
