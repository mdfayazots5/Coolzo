namespace Coolzo.Contracts.Responses.FieldExecution;

public sealed record JobAttachmentResponse(
    long JobAttachmentId,
    string AttachmentType,
    string FileName,
    string ContentType,
    long FileSizeInBytes,
    string FileUrl,
    string AttachmentRemarks,
    DateTime UploadedDateUtc);
