namespace Coolzo.Domain.Entities;

public sealed class PricingModel : AuditableEntity
{
    public long PricingModelId { get; set; }

    public string PricingModelName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Service> Services { get; set; } = new List<Service>();
}
