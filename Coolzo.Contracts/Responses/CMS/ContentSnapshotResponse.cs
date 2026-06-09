namespace Coolzo.Contracts.Responses.CMS;

/// <summary>
/// The full published content snapshot document written to object storage and consumed by the
/// public Web portal. Serialized as camelCase JSON. Keys follow the SnapshotKeys registry.
/// </summary>
public sealed record ContentSnapshotResponse(
    int Version,
    DateTime PublishedAtUtc,
    string Checksum,
    IReadOnlyDictionary<string, string> Theme,
    SnapshotMastersDto Masters,
    SnapshotContentDto Content,
    IReadOnlyDictionary<string, SnapshotImageDto> Images);

/// <summary>The aggregated snapshot body (everything except the version/checksum envelope).</summary>
public sealed record ContentSnapshotBody(
    IReadOnlyDictionary<string, string> Theme,
    SnapshotMastersDto Masters,
    SnapshotContentDto Content,
    IReadOnlyDictionary<string, SnapshotImageDto> Images);

public sealed record SnapshotMastersDto(
    IReadOnlyCollection<SnapshotBrandDto> Brands);

public sealed record SnapshotBrandDto(
    string Code,
    string Name);

public sealed record SnapshotContentDto(
    IReadOnlyCollection<SnapshotBlockDto> Blocks,
    IReadOnlyCollection<SnapshotBannerDto> Banners,
    IReadOnlyCollection<SnapshotFaqDto> Faqs);

public sealed record SnapshotBlockDto(
    string Key,
    string Title,
    string Summary,
    string Content,
    string PreviewImageUrl,
    int SortOrder);

public sealed record SnapshotBannerDto(
    string Title,
    string Subtitle,
    string ImageUrl,
    string RedirectUrl,
    string DisplayArea,
    int SortOrder);

public sealed record SnapshotFaqDto(
    string Category,
    string Question,
    string Answer,
    int SortOrder);

public sealed record SnapshotImageDto(
    string Url,
    string Alt,
    IReadOnlyDictionary<string, string> Variants);

/// <summary>Lightweight pointer to the active snapshot. Served from object storage; cached for fallback.</summary>
public sealed record SnapshotManifestResponse(
    int Version,
    string BucketUrl,
    string Checksum,
    DateTime PublishedAtUtc);
