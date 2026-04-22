using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class JobReport : AuditableEntity
{
    public long JobReportId { get; set; }

    public long ServiceRequestId { get; set; }

    public long JobCardId { get; set; }

    public long TechnicianId { get; set; }

    public string IdempotencyKey { get; set; } = string.Empty;

    public string EquipmentCondition { get; set; } = string.Empty;

    public string IssuesIdentifiedJson { get; set; } = "[]";

    public string ActionTaken { get; set; } = string.Empty;

    public string Recommendation { get; set; } = string.Empty;

    public string Observations { get; set; } = string.Empty;

    public DateTime SubmittedAtUtc { get; set; }

    public bool IsQualityReviewed { get; set; }

    public decimal QualityScore { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }

    public JobCard? JobCard { get; set; }

    public Technician? Technician { get; set; }

    public ICollection<JobPhoto> Photos { get; set; } = new List<JobPhoto>();

    public ICollection<CustomerSignature> Signatures { get; set; } = new List<CustomerSignature>();
}

public sealed class JobPhoto : AuditableEntity
{
    public long JobPhotoId { get; set; }

    public long ServiceRequestId { get; set; }

    public long JobCardId { get; set; }

    public long TechnicianId { get; set; }

    public long? JobReportId { get; set; }

    public JobPhotoType PhotoType { get; set; } = JobPhotoType.Before;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string StorageUrl { get; set; } = string.Empty;

    public string UploadedBy { get; set; } = string.Empty;

    public DateTime UploadedAtUtc { get; set; }

    public string PhotoRemarks { get; set; } = string.Empty;

    public ServiceRequest? ServiceRequest { get; set; }

    public JobCard? JobCard { get; set; }

    public Technician? Technician { get; set; }

    public JobReport? JobReport { get; set; }
}

public sealed class CustomerSignature : AuditableEntity
{
    public long CustomerSignatureId { get; set; }

    public long ServiceRequestId { get; set; }

    public long JobCardId { get; set; }

    public long TechnicianId { get; set; }

    public long? JobReportId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string SignatureDataUrl { get; set; } = string.Empty;

    public DateTime SignedAtUtc { get; set; }

    public string CapturedBy { get; set; } = string.Empty;

    public string SignatureRemarks { get; set; } = string.Empty;

    public ServiceRequest? ServiceRequest { get; set; }

    public JobCard? JobCard { get; set; }

    public Technician? Technician { get; set; }

    public JobReport? JobReport { get; set; }
}

public sealed class PartsRequest : AuditableEntity
{
    public long PartsRequestId { get; set; }

    public long ServiceRequestId { get; set; }

    public long JobCardId { get; set; }

    public long TechnicianId { get; set; }

    public PartsRequestUrgency Urgency { get; set; } = PartsRequestUrgency.Normal;

    public PartsRequestStatus CurrentStatus { get; set; } = PartsRequestStatus.Pending;

    public string Notes { get; set; } = string.Empty;

    public DateTime SubmittedAtUtc { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }

    public JobCard? JobCard { get; set; }

    public Technician? Technician { get; set; }

    public ICollection<PartsRequestItem> Items { get; set; } = new List<PartsRequestItem>();
}

public sealed class PartsRequestItem : AuditableEntity
{
    public long PartsRequestItemId { get; set; }

    public long PartsRequestId { get; set; }

    public string PartCode { get; set; } = string.Empty;

    public string PartName { get; set; } = string.Empty;

    public decimal QuantityRequested { get; set; }

    public decimal QuantityApproved { get; set; }

    public string ItemRemarks { get; set; } = string.Empty;

    public PartsRequestStatus CurrentStatus { get; set; } = PartsRequestStatus.Pending;

    public PartsRequest? PartsRequest { get; set; }
}
