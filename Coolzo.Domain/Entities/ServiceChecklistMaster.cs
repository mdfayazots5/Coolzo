namespace Coolzo.Domain.Entities;

public sealed class ServiceChecklistMaster : AuditableEntity
{
    public long ServiceChecklistMasterId { get; set; }

    public long ServiceId { get; set; }

    public string ChecklistTitle { get; set; } = string.Empty;

    public string ChecklistDescription { get; set; } = string.Empty;

    public bool IsMandatory { get; set; }

    public Service? Service { get; set; }

    public ICollection<JobChecklistResponse> ChecklistResponses { get; set; } = new List<JobChecklistResponse>();
}
