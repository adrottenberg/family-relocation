using Amazon.S3;
using Amazon.S3.Model;
using FamilyRelocation.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FamilyRelocation.Infrastructure.AWS;

/// <summary>
/// S3 implementation of document storage service.
/// This is a simple storage abstraction - naming conventions are handled by the caller.
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
        string storageKey,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = storageKey,
            InputStream = fileStream,
            ContentType = contentType,
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        var url = $"https://{_bucketName}.s3.amazonaws.com/{storageKey}";

        return new DocumentUploadResult(
            StorageUrl: url,
            StorageKey: storageKey,
            FileSize: fileStream.Length,
            ContentType: contentType,
            UploadedAt: DateTime.UtcNow);
    }

    /// <inheritdoc />
    public Task<string> GetPreSignedUrlAsync(
        string storageKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = storageKey,
            Expires = DateTime.UtcNow.Add(expiry),
        };

        var url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(url);
    }

    /// <inheritdoc />
    public async Task<DocumentDownloadResult?> DownloadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = storageKey
            };

            var response = await _s3Client.GetObjectAsync(request, cancellationToken);

            return new DocumentDownloadResult(
                Content: response.ResponseStream,
                ContentType: response.Headers.ContentType,
                ContentLength: response.Headers.ContentLength,
                ETag: response.ETag);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
