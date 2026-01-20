using Amazon.S3;
using Amazon.S3.Model;
using FamilyRelocation.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FamilyRelocation.Infrastructure.AWS;

/// <summary>
/// S3 implementation of document storage service.
/// </summary>
public class S3DocumentStorageService : IDocumentStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3DocumentStorageService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:S3:BucketName"]
            ?? throw new InvalidOperationException("AWS:S3:BucketName configuration is required");
    }

    /// <inheritdoc />
    public async Task<DocumentUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Guid applicantId,
        string documentType,
        CancellationToken cancellationToken = default)
    {
        // Sanitize document type for use in path
        var safeDocumentType = documentType.ToLowerInvariant().Replace(" ", "-");

        // Generate unique key with timestamp to avoid overwrites
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HHmmss");
        var safeFileName = SanitizeFileName(fileName);
        var key = $"documents/{applicantId}/{safeDocumentType}/{timestamp}_{safeFileName}";

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        var url = $"https://{_bucketName}.s3.amazonaws.com/{key}";

        return new DocumentUploadResult(
            DocumentUrl: url,
            DocumentKey: key,
            FileName: fileName,
            FileSize: fileStream.Length,
            ContentType: contentType,
            UploadedAt: DateTime.UtcNow);
    }

    /// <inheritdoc />
    public Task<string> GetPreSignedUrlAsync(
        string documentKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = documentKey,
            Expires = DateTime.UtcNow.Add(expiry),
        };

        var url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(url);
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove or replace characters that might cause issues in S3 keys
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Replace(" ", "_");
    }
}
