namespace Coolzo.Domain.Entities;

public sealed class JobExecutionNote : AuditableEntity
{
    public long JobExecutionNoteId { get; set; }

    public long JobCardId { get; set; }

    public string NoteText { get; set; } = string.Empty;

    public bool IsCustomerVisible { get; set; }

    public DateTime NoteDateUtc { get; set; }

    public JobCard? JobCard { get; set; }
}
