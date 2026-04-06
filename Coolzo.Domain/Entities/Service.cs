namespace Coolzo.Domain.Entities;

public sealed class Service : AuditableEntity
{
    public long ServiceId { get; set; }

    public long ServiceCategoryId { get; set; }

    public long PricingModelId { get; set; }

    public string ServiceCode { get; set; } = string.Empty;

    public string ServiceName { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public int EstimatedDurationInMinutes { get; set; }

    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;

    public ServiceCategory? ServiceCategory { get; set; }

    public PricingModel? PricingModel { get; set; }

    public ICollection<BookingLine> BookingLines { get; set; } = new List<BookingLine>();
}
