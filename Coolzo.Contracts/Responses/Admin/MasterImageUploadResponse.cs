namespace Coolzo.Contracts.Responses.Admin;

/// <summary>
/// Public URL of an uploaded master-data image. The caller persists this URL into the
/// owning master record's metadata (e.g. service-type metadata.imageUrl).
/// </summary>
public sealed record MasterImageUploadResponse(string Url);
