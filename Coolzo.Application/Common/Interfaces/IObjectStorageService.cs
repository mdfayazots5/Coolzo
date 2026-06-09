using Coolzo.Application.Common.Models;

namespace Coolzo.Application.Common.Interfaces;

/// <summary>
/// General-purpose object storage abstraction for the Render storage bucket / persistent disk.
/// Used by the CMS Content & Theme Delivery module to publish the static content snapshot and
/// to store uploaded screen images. Provider-agnostic: a filesystem implementation backs it today;
/// an S3 / Render-bucket implementation can replace it without changing callers.
/// </summary>
public interface IObjectStorageService
{
    /// <summary>
    /// Writes (or overwrites) an object at the given key and returns its public URL.
    /// </summary>
    Task<StoredObjectResult> PutObjectAsync(
        string key,
        string contentType,
        byte[] content,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reads an object by key. Returns null when the object does not exist.
    /// </summary>
    Task<byte[]?> GetObjectAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Returns true when an object exists at the given key.
    /// </summary>
    Task<bool> ObjectExistsAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the public URL a consumer (e.g. the public Web portal) uses to fetch the object.
    /// </summary>
    string GetPublicUrl(string key);

    /// <summary>
    /// Verifies the storage backend is reachable and writable. Used by the health check.
    /// </summary>
    Task<bool> CheckWritableAsync(CancellationToken cancellationToken);
}
