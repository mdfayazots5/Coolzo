using Microsoft.Extensions.Configuration;

namespace Coolzo.Infrastructure.Storage;

/// <summary>
/// Startup fail-fast guard for object storage. CMS object storage runs exclusively on Cloudflare R2
/// (S3-compatible); the filesystem provider was removed because Render's disk is ephemeral and loses
/// uploaded images and published snapshots on redeploy. This guard refuses to start the host unless
/// the R2 settings are complete, so a misconfiguration surfaces immediately instead of at first upload.
/// </summary>
public static class ObjectStorageConfigurationGuard
{
    public static void Validate(IConfiguration configuration)
    {
        var section = configuration.GetSection(ObjectStorageOptions.SectionName);
        var options = section.Get<ObjectStorageOptions>() ?? new ObjectStorageOptions();

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(options.S3.ServiceUrl))
        {
            missing.Add($"{ObjectStorageOptions.SectionName}:S3:ServiceUrl");
        }

        if (string.IsNullOrWhiteSpace(options.S3.BucketName))
        {
            missing.Add($"{ObjectStorageOptions.SectionName}:S3:BucketName");
        }

        if (string.IsNullOrWhiteSpace(options.S3.AccessKey))
        {
            missing.Add($"{ObjectStorageOptions.SectionName}:S3:AccessKey");
        }

        if (string.IsNullOrWhiteSpace(options.S3.SecretKey))
        {
            missing.Add($"{ObjectStorageOptions.SectionName}:S3:SecretKey");
        }

        if (string.IsNullOrWhiteSpace(options.PublicBaseUrl))
        {
            missing.Add($"{ObjectStorageOptions.SectionName}:PublicBaseUrl");
        }

        if (string.IsNullOrWhiteSpace(options.JobMediaBucketName))
        {
            missing.Add($"{ObjectStorageOptions.SectionName}:JobMediaBucketName");
        }

        if (string.IsNullOrWhiteSpace(options.JobMediaAccessKey))
        {
            missing.Add($"{ObjectStorageOptions.SectionName}:JobMediaAccessKey");
        }

        if (string.IsNullOrWhiteSpace(options.JobMediaSecretKey))
        {
            missing.Add($"{ObjectStorageOptions.SectionName}:JobMediaSecretKey");
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                "CMS object storage (Cloudflare R2) is missing required settings: " +
                $"{string.Join(", ", missing)}. Supply these via environment variables / user-secrets " +
                "(never source control) before the application can start. AccessKey and SecretKey come " +
                "from a Cloudflare R2 API token scoped to the bucket with Object Read & Write.");
        }
    }
}
