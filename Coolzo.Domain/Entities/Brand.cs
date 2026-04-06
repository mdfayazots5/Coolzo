namespace Coolzo.Domain.Entities;

public sealed class Brand : AuditableEntity
{
    public long BrandId { get; set; }

    public string BrandCode { get; set; } = string.Empty;

    public string BrandName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<BookingLine> BookingLines { get; set; } = new List<BookingLine>();
}
