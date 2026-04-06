namespace Coolzo.Domain.Entities;

public sealed class JobChecklistResponse : AuditableEntity
{
    public long JobChecklistResponseId { get; set; }

    public long JobCardId { get; set; }

    public long ServiceChecklistMasterId { get; set; }

    public bool? IsChecked { get; set; }

    public string ResponseRemarks { get; set; } = string.Empty;

    public DateTime ResponseDateUtc { get; set; }

    public JobCard? JobCard { get; set; }

    public ServiceChecklistMaster? ServiceChecklistMaster { get; set; }
}
