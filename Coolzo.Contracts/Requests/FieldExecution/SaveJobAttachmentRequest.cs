namespace Coolzo.Contracts.Requests.FieldExecution;

public sealed record SaveJobAttachmentRequest(
    string AttachmentType,
    string FileName,
    string ContentType,
    string Base64Content,
    string? AttachmentRemarks);
