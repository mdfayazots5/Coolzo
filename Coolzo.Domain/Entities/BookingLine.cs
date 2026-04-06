namespace Coolzo.Domain.Entities;

public sealed class BookingLine : AuditableEntity
{
    public long BookingLineId { get; set; }

    public long BookingId { get; set; }

    public long ServiceId { get; set; }

    public long AcTypeId { get; set; }

    public long TonnageId { get; set; }

    public long BrandId { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string IssueNotes { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    public Booking? Booking { get; set; }

    public Service? Service { get; set; }

    public AcType? AcType { get; set; }

    public Tonnage? Tonnage { get; set; }

    public Brand? Brand { get; set; }
}
