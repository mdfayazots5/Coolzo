namespace Coolzo.Domain.Entities;

/// <summary>
/// A published content snapshot of the public Web portal. Each publish produces a new, immutable,
/// versioned row pointing to the static JSON artifact in object storage. Exactly one row is active
/// at a time; prior versions are retained for rollback and audit.
/// </summary>
public sealed class PublishedSnapshot : AuditableEntity
{
    public long PublishedSnapshotId { get; set; }

    public int Version { get; set; }

    public string BucketKey { get; set; } = string.Empty;

    public string BucketUrl { get; set; } = string.Empty;

    public string ChecksumHash { get; set; } = string.Empty;

    public long PayloadSizeBytes { get; set; }

    public bool IsActive { get; set; }
}
