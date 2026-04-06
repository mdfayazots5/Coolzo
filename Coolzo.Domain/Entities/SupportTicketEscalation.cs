namespace Coolzo.Domain.Entities;

public sealed class SupportTicketEscalation : AuditableEntity
{
    public long SupportTicketEscalationId { get; set; }

    public long SupportTicketId { get; set; }

    public string EscalationTarget { get; set; } = string.Empty;

    public string EscalationRemarks { get; set; } = string.Empty;

    public DateTime EscalatedDateUtc { get; set; }

    public SupportTicket? SupportTicket { get; set; }
}
