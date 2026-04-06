namespace Coolzo.Domain.Entities;

public sealed class SupportTicketCategory : AuditableEntity
{
    public long SupportTicketCategoryId { get; set; }

    public string CategoryCode { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
}
