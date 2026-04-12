namespace Coolzo.Domain.Entities;

public sealed class CustomerAddress : AuditableEntity
{
    public long CustomerAddressId { get; set; }

    public long CustomerId { get; set; }

    public long ZoneId { get; set; }

    public string AddressLabel { get; set; } = string.Empty;

    public string AddressLine1 { get; set; } = string.Empty;

    public string AddressLine2 { get; set; } = string.Empty;

    public string Landmark { get; set; } = string.Empty;

    public string CityName { get; set; } = string.Empty;

    public string StateName { get; set; } = string.Empty;

    public string Pincode { get; set; } = string.Empty;

    public string AddressType { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    public Customer? Customer { get; set; }

    public Zone? Zone { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
