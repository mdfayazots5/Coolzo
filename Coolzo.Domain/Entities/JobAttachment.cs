using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class JobAttachment : AuditableEntity
{
    public long JobAttachmentId { get; set; }

    public long JobCardId { get; set; }

    public JobAttachmentType AttachmentType { get; set; } = JobAttachmentType.AdditionalPhoto;

    public string FileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeInBytes { get; set; }

    public string AttachmentRemarks { get; set; } = string.Empty;

    public DateTime UploadedDateUtc { get; set; }

    public JobCard? JobCard { get; set; }
}
