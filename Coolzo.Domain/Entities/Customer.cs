namespace Coolzo.Domain.Entities;

public sealed class Customer : AuditableEntity
{
    public long CustomerId { get; set; }

    public long? UserId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string EmailAddress { get; set; } = string.Empty;

    public bool IsGuestCustomer { get; set; }

    public bool IsActive { get; set; } = true;

    public User? User { get; set; }

    public ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public ICollection<QuotationHeader> Quotations { get; set; } = new List<QuotationHeader>();

    public ICollection<InvoiceHeader> Invoices { get; set; } = new List<InvoiceHeader>();
}
