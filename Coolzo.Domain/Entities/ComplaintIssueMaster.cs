namespace Coolzo.Domain.Entities;

public sealed class ComplaintIssueMaster : AuditableEntity
{
    public long ComplaintIssueMasterId { get; set; }

    public long? ServiceId { get; set; }

    public string IssueName { get; set; } = string.Empty;

    public string IssueDescription { get; set; } = string.Empty;

    public Service? Service { get; set; }

    public ICollection<JobDiagnosis> JobDiagnoses { get; set; } = new List<JobDiagnosis>();
}
