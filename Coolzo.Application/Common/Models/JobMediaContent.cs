namespace Coolzo.Application.Common.Models;

/// <summary>
/// Raw bytes + content type of a job-media object streamed back from private object storage through
/// the API proxy. Returned by <see cref="Coolzo.Application.Common.Interfaces.IJobAttachmentStorageService"/>.
/// </summary>
public sealed record JobMediaContent(
    byte[] Content,
    string ContentType);
