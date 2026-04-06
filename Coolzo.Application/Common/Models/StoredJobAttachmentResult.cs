namespace Coolzo.Application.Common.Models;

public sealed record StoredJobAttachmentResult(
    string StoredFileName,
    string RelativePath,
    long FileSizeInBytes);
