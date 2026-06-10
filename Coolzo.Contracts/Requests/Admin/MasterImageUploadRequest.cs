namespace Coolzo.Contracts.Requests.Admin;

/// <summary>
/// Uploads a master-data image (e.g. a service catalog image) to object storage.
/// <see cref="Folder"/> groups the asset under catalog/{folder}/ (e.g. "service-types").
/// </summary>
public sealed record MasterImageUploadRequest(
    string Folder,
    string FileName,
    string ContentType,
    string Base64Content);
