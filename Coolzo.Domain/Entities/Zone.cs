namespace Coolzo.Domain.Entities;

public sealed class Zone : AuditableEntity
{
    public long ZoneId { get; set; }

    public string ZoneCode { get; set; } = string.Empty;

    public string ZoneName { get; set; } = string.Empty;

    public string CityName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<ZonePincode> ZonePincodes { get; set; } = new List<ZonePincode>();

    public ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();

    public ICollection<SlotConfiguration> SlotConfigurations { get; set; } = new List<SlotConfiguration>();

    public ICollection<SlotAvailability> SlotAvailabilities { get; set; } = new List<SlotAvailability>();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
