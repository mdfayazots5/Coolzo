using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class BookingStatusHistory : AuditableEntity
{
    public long BookingStatusHistoryId { get; set; }

    public long BookingId { get; set; }

    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

    public string Remarks { get; set; } = string.Empty;

    public DateTime StatusDateUtc { get; set; }

    public Booking? Booking { get; set; }
}
