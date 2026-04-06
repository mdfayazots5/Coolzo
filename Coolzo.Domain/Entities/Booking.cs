using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class Booking : AuditableEntity
{
    public long BookingId { get; set; }

    public string BookingReference { get; set; } = string.Empty;

    public string? IdempotencyKey { get; set; }

    public long CustomerId { get; set; }

    public long CustomerAddressId { get; set; }

    public long ZoneId { get; set; }

    public long SlotAvailabilityId { get; set; }

    public DateTime BookingDateUtc { get; set; }

    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

    public BookingSourceChannel SourceChannel { get; set; } = BookingSourceChannel.Web;

    public bool IsGuestBooking { get; set; }

    public string CustomerNameSnapshot { get; set; } = string.Empty;

    public string MobileNumberSnapshot { get; set; } = string.Empty;

    public string EmailAddressSnapshot { get; set; } = string.Empty;

    public string AddressLine1Snapshot { get; set; } = string.Empty;

    public string AddressLine2Snapshot { get; set; } = string.Empty;

    public string LandmarkSnapshot { get; set; } = string.Empty;

    public string CityNameSnapshot { get; set; } = string.Empty;

    public string PincodeSnapshot { get; set; } = string.Empty;

    public string ZoneNameSnapshot { get; set; } = string.Empty;

    public string ServiceNameSnapshot { get; set; } = string.Empty;

    public decimal EstimatedPrice { get; set; }

    public Customer? Customer { get; set; }

    public CustomerAddress? CustomerAddress { get; set; }

    public Zone? Zone { get; set; }

    public SlotAvailability? SlotAvailability { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }

    public ICollection<BookingLine> BookingLines { get; set; } = new List<BookingLine>();

    public ICollection<BookingStatusHistory> BookingStatusHistories { get; set; } = new List<BookingStatusHistory>();
}
