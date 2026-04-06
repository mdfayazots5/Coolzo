using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class SupportTicketStatusHistory : AuditableEntity
{
    public long SupportTicketStatusHistoryId { get; set; }

    public long SupportTicketId { get; set; }

    public SupportTicketStatus SupportTicketStatus { get; set; }

    public string Remarks { get; set; } = string.Empty;

    public DateTime StatusDateUtc { get; set; }

    public SupportTicket? SupportTicket { get; set; }
}
