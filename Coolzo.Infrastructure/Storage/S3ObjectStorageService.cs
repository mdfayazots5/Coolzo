using Amazon.S3;
using Amazon.S3.Model;
using Coolzo.Application.Common.Interfaces;
using Coolzo.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace Coolzo.Infrastructure.Storage;

/// <summary>
/// S3-compatible object storage (AWS S3, Cloudflare R2, Backblaze B2, MinIO, …). Credentials and
/// endpoint come from configuration / environment — never hardcoded.
/// </summary>
public sealed class S3ObjectStorageService : IObjectStorageService
{
    private const string HealthProbeKey = "cms/.storage-health-probe";

    private readonly IAmazonS3 _s3Client;
    private readonly ObjectStorageOptions _options;

    public S3ObjectStorageService(IAmazonS3 s3Client, IOptions<ObjectStorageOptions> options)
    {
        _s3Client = s3Client;
        _options = options.Value;
    }

    public async Task<StoredObjectResult> PutObjectAsync(
        string key,
        string contentType,
        byte[] content,
        CancellationToken cancellationToken)
    {
        var normalizedKey = NormalizeKey(key);

        using var stream = new MemoryStream(content);
        var request = new PutObjectRequest
        {
            BucketName = _options.S3.BucketName,
            Key = normalizedKey,
            InputStream = stream,
            ContentType = contentType,
            AutoCloseStream = false
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        return new StoredObjectResult(normalizedKey, GetPublicUrl(normalizedKey), content.LongLength, contentType);
    }

    public async Task<byte[]?> GetObjectAsync(string key, CancellationToken cancellationToken)
    {
        var normalizedKey = NormalizeKey(key);

        try
        {
            using var response = await _s3Client.GetObjectAsync(_options.S3.BucketName, normalizedKey, cancellationToken);
            using var buffer = new MemoryStream();
            await response.ResponseStream.CopyToAsync(buffer, cancellationToken);
            return buffer.ToArray();
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> ObjectExistsAsync(string key, CancellationToken cancellationToken)
    {
        var normalizedKey = NormalizeKey(key);

        try
        {
            await _s3Client.GetObjectMetadataAsync(_options.S3.BucketName, normalizedKey, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public string GetPublicUrl(string key)
    {
        var normalizedKey = NormalizeKey(key);

        return string.IsNullOrWhiteSpace(_options.PublicBaseUrl)
            ? $"/{normalizedKey}"
            : $"{_options.PublicBaseUrl.TrimEnd('/')}/{normalizedKey}";
    }

    public async Task<bool> CheckWritableAsync(CancellationToken cancellationToken)
    {
        try
        {
            var probe = new byte[] { 0x6F, 0x6B };
            await PutObjectAsync(HealthProbeKey, "application/octet-stream", probe, cancellationToken);
            await _s3Client.DeleteObjectAsync(_options.S3.BucketName, NormalizeKey(HealthProbeKey), cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Object storage key must not be empty.", nameof(key));
        }

        var trimmed = key.Replace('\\', '/').TrimStart('/');

        if (trimmed.Contains("..", StringComparison.Ordinal))
        {
            throw new ArgumentException("Object storage key must not contain path traversal.", nameof(key));
        }

        return trimmed;
    }
}
