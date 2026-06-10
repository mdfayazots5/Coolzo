namespace Coolzo.Contracts.Requests.Admin;

/// <summary>
/// Sets (or clears, when null/empty) the per-service image URL on a bookable tblService row.
/// The URL is produced by POST /api/admin-masters/upload-image (folder="services").
/// </summary>
public sealed record SetServiceImageRequest(string? ImageUrl);
