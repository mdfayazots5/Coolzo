using Amazon.S3;

namespace Coolzo.Infrastructure.Storage;

/// <summary>
/// Typed wrapper around the S3 client for the PRIVATE job-media bucket. Distinguishes it from the
/// default <see cref="IAmazonS3"/> (the public CMS bucket client) in the container so each bucket uses
/// its own least-privilege Cloudflare R2 token.
/// </summary>
public sealed class JobMediaStorageClient
{
    public JobMediaStorageClient(IAmazonS3 client)
    {
        Client = client;
    }

    public IAmazonS3 Client { get; }
}
