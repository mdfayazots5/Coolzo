namespace Coolzo.Infrastructure.Storage;

public sealed class ObjectStorageOptions
{
    public const string SectionName = "ObjectStorage";

    /// <summary>
    /// Public URL prefix objects are served from — the R2 bucket's public/CDN base URL. Combined with
    /// the object key to form the absolute URL stored on each image/snapshot record.
    /// </summary>
    public string PublicBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// PRIVATE R2 bucket for job/technician media (customer site photos — PII-grade). It is NOT public:
    /// objects are streamed back through the API proxy, never via a public URL. Uses the same account
    /// endpoint as <see cref="S3"/> but its OWN least-privilege token (JobMediaAccessKey/JobMediaSecretKey).
    /// </summary>
    public string JobMediaBucketName { get; set; } = string.Empty;

    /// <summary>Access key for the private job-media R2 token. Secret — supply via env / user-secrets.</summary>
    public string JobMediaAccessKey { get; set; } = string.Empty;

    /// <summary>Secret key for the private job-media R2 token. Secret — supply via env / user-secrets.</summary>
    public string JobMediaSecretKey { get; set; } = string.Empty;

    /// <summary>Cloudflare R2 (S3-compatible) bucket settings.</summary>
    public S3StorageOptions S3 { get; set; } = new();
}

public sealed class S3StorageOptions
{
    /// <summary>Account endpoint for the S3-compatible provider, e.g. https://&lt;account&gt;.r2.cloudflarestorage.com.</summary>
    public string ServiceUrl { get; set; } = string.Empty;

    public string Region { get; set; } = "auto";

    public string BucketName { get; set; } = string.Empty;

    /// <summary>Secret — supply via environment variable / user-secrets, never source control.</summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>Secret — supply via environment variable / user-secrets, never source control.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Required true for R2 (and most S3-compatible providers); false only for native AWS S3.</summary>
    public bool ForcePathStyle { get; set; } = true;
}
