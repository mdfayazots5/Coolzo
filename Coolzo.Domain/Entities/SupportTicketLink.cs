using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class SupportTicketLink : AuditableEntity
{
    public long SupportTicketLinkId { get; set; }

    public long SupportTicketId { get; set; }

    public SupportTicketLinkType LinkedEntityType { get; set; }

    public long LinkedEntityId { get; set; }

    public string LinkReference { get; set; } = string.Empty;

    public string LinkSummary { get; set; } = string.Empty;

    public SupportTicket? SupportTicket { get; set; }
}
