using Amazon.S3;
using Amazon.S3.Model;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace Coolzo.Infrastructure.Storage;

/// <summary>
/// Job/technician media storage backed by a PRIVATE Cloudflare R2 (S3-compatible) bucket. Customer
/// site photos are PII-grade and must never be publicly readable, so this writes to a separate private
/// bucket and returns a relative API-proxy URL ("/api/field-media/{key}") that the API streams on read.
/// This keeps existing callers and the mobile contract unchanged while moving bytes off Render's
/// ephemeral disk (the old LocalJobAttachmentStorageService) into durable storage.
/// </summary>
public sealed class R2JobAttachmentStorageService : IJobAttachmentStorageService
{
    private const string KeyPrefix = "job-media";

    private readonly IAmazonS3 _s3Client;
    private readonly ObjectStorageOptions _options;

    public R2JobAttachmentStorageService(JobMediaStorageClient jobMediaClient, IOptions<ObjectStorageOptions> options)
    {
        _s3Client = jobMediaClient.Client;
        _options = options.Value;
    }

    public string ProxyBasePath => "/api/field-media/";

    public async Task<StoredJobAttachmentResult> SaveAsync(
        string fileName,
        string contentType,
        byte[] fileBytes,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}".ToLowerInvariant();
        var objectKey = $"{KeyPrefix}/{DateTime.UtcNow:yyyyMMdd}/{storedFileName}";

        using var stream = new MemoryStream(fileBytes);
        var request = new PutObjectRequest
        {
            BucketName = _options.JobMediaBucketName,
            Key = objectKey,
            InputStream = stream,
            ContentType = contentType,
            AutoCloseStream = false,
            // Cloudflare R2 does not implement chunked streaming payload signing
            // (STREAMING-AWS4-HMAC-SHA256-PAYLOAD). Send the body as UNSIGNED-PAYLOAD over HTTPS
            // instead so R2 accepts the upload.
            DisablePayloadSigning = true
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        // RelativePath is the relative API proxy URL callers persist and return; the bytes themselves
        // live in the private bucket and are only reachable through GetByKeyAsync via the proxy endpoint.
        var relativeProxyUrl = $"{ProxyBasePath}{objectKey}";

        return new StoredJobAttachmentResult(storedFileName, relativeProxyUrl, fileBytes.LongLength);
    }

    public async Task<JobMediaContent?> GetByKeyAsync(string objectKey, CancellationToken cancellationToken)
    {
        var normalizedKey = NormalizeKey(objectKey);

        try
        {
            using var response = await _s3Client.GetObjectAsync(_options.JobMediaBucketName, normalizedKey, cancellationToken);
            using var buffer = new MemoryStream();
            await response.ResponseStream.CopyToAsync(buffer, cancellationToken);

            var contentType = string.IsNullOrWhiteSpace(response.Headers.ContentType)
                ? "application/octet-stream"
                : response.Headers.ContentType;

            return new JobMediaContent(buffer.ToArray(), contentType);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <summary>
    /// Validates the requested key: relative, no traversal, and confined to the job-media prefix so the
    /// proxy can never be coaxed into reading an arbitrary object from the bucket.
    /// </summary>
    private static string NormalizeKey(string objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            throw new ArgumentException("Job media key must not be empty.", nameof(objectKey));
        }

        var trimmed = objectKey.Replace('\\', '/').TrimStart('/');

        if (trimmed.Contains("..", StringComparison.Ordinal) || Path.IsPathRooted(trimmed))
        {
            throw new ArgumentException("Job media key must be a relative path without traversal.", nameof(objectKey));
        }

        if (!trimmed.StartsWith($"{KeyPrefix}/", StringComparison.Ordinal))
        {
            throw new ArgumentException("Job media key is outside the permitted prefix.", nameof(objectKey));
        }

        return trimmed;
    }
}
