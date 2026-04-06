namespace Coolzo.Domain.Entities;

public sealed class JobDiagnosis : AuditableEntity
{
    public long JobDiagnosisId { get; set; }

    public long JobCardId { get; set; }

    public long? ComplaintIssueMasterId { get; set; }

    public long? DiagnosisResultMasterId { get; set; }

    public string DiagnosisRemarks { get; set; } = string.Empty;

    public DateTime DiagnosisDateUtc { get; set; }

    public JobCard? JobCard { get; set; }

    public ComplaintIssueMaster? ComplaintIssueMaster { get; set; }

    public DiagnosisResultMaster? DiagnosisResultMaster { get; set; }
}
