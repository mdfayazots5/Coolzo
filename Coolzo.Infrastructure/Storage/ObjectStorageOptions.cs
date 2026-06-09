namespace Coolzo.Infrastructure.Storage;

public sealed class ObjectStorageOptions
{
    public const string SectionName = "ObjectStorage";

    public const string FileSystemProvider = "FileSystem";
    public const string S3Provider = "S3";

    /// <summary>Active storage provider: "FileSystem" (default) or "S3" (S3-compatible bucket).</summary>
    public string Provider { get; set; } = FileSystemProvider;

    /// <summary>
    /// FileSystem root. Relative paths resolve under the application content root (e.g. "wwwroot");
    /// an absolute path targets a mounted disk. Ignored when Provider = "S3".
    /// </summary>
    public string RootPath { get; set; } = "wwwroot";

    /// <summary>
    /// Public URL prefix objects are served from. When empty, a server-relative URL ("/{key}") is
    /// returned. For S3/CDN set this to the bucket's public/CDN base URL.
    /// </summary>
    public string PublicBaseUrl { get; set; } = string.Empty;

    /// <summary>S3-compatible bucket settings. Used only when Provider = "S3".</summary>
    public S3StorageOptions S3 { get; set; } = new();
}

public sealed class S3StorageOptions
{
    /// <summary>Custom endpoint for S3-compatible providers (e.g. Cloudflare R2, Backblaze). Leave empty for AWS S3.</summary>
    public string ServiceUrl { get; set; } = string.Empty;

    public string Region { get; set; } = "us-east-1";

    public string BucketName { get; set; } = string.Empty;

    /// <summary>Secret — supply via environment variable / user-secrets, never source control.</summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>Secret — supply via environment variable / user-secrets, never source control.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Required true for most S3-compatible providers (R2, MinIO); false for native AWS S3.</summary>
    public bool ForcePathStyle { get; set; } = true;
}
