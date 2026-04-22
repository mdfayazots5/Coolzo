namespace Coolzo.Domain.Entities;

public sealed class JobCard : AuditableEntity
{
    public long JobCardId { get; set; }

    public string JobCardNumber { get; set; } = string.Empty;

    public long ServiceRequestId { get; set; }

    public DateTime? WorkStartedDateUtc { get; set; }

    public DateTime? WorkInProgressDateUtc { get; set; }

    public DateTime? WorkCompletedDateUtc { get; set; }

    public DateTime? SubmittedForClosureDateUtc { get; set; }

    public string CompletionSummary { get; set; } = string.Empty;

    public ServiceRequest? ServiceRequest { get; set; }

    public JobDiagnosis? JobDiagnosis { get; set; }

    public ICollection<JobChecklistResponse> ChecklistResponses { get; set; } = new List<JobChecklistResponse>();

    public ICollection<JobAttachment> Attachments { get; set; } = new List<JobAttachment>();

    public ICollection<JobReport> JobReports { get; set; } = new List<JobReport>();

    public ICollection<JobPhoto> JobPhotos { get; set; } = new List<JobPhoto>();

    public ICollection<CustomerSignature> CustomerSignatures { get; set; } = new List<CustomerSignature>();

    public ICollection<JobExecutionNote> ExecutionNotes { get; set; } = new List<JobExecutionNote>();

    public ICollection<JobExecutionTimeline> ExecutionTimelines { get; set; } = new List<JobExecutionTimeline>();

    public ICollection<QuotationHeader> Quotations { get; set; } = new List<QuotationHeader>();

    public ICollection<JobPartConsumption> PartConsumptions { get; set; } = new List<JobPartConsumption>();

    public ICollection<PartsRequest> PartsRequests { get; set; } = new List<PartsRequest>();
}
