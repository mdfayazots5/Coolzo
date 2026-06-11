namespace Coolzo.Contracts.Responses.CMS;

/// <summary>Result of uploading a standalone CMS asset (e.g. brand logo). Returns the public URL to store in a theme token.</summary>
public sealed record CmsAssetUploadResponse(string ImageUrl);
