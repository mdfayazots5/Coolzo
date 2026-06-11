using Coolzo.Application.Common.Models;

namespace Coolzo.Application.Common.Interfaces;

/// <summary>
/// Storage for job/technician media (customer site photos — PII-grade) in a PRIVATE object-storage
/// bucket. Objects are never publicly readable; <see cref="GetByKeyAsync"/> streams them back through
/// the authenticated API proxy. <see cref="SaveAsync"/> returns a relative proxy URL (in RelativePath)
/// that existing callers persist and return unchanged — the immutable mobile contract field shape is
/// preserved while the bytes move off ephemeral local disk into durable storage.
/// </summary>
public interface IJobAttachmentStorageService
{
    Task<StoredJobAttachmentResult> SaveAsync(
        string fileName,
        string contentType,
        byte[] fileBytes,
        CancellationToken cancellationToken);

    /// <summary>
    /// Fetches a stored job-media object by its storage key. Returns null when the object does not
    /// exist. The key is validated against the job-media prefix to prevent reading arbitrary objects.
    /// </summary>
    Task<JobMediaContent?> GetByKeyAsync(string objectKey, CancellationToken cancellationToken);

    /// <summary>The relative API path that proxies job-media reads, e.g. "/api/field-media/".</summary>
    string ProxyBasePath { get; }
}
