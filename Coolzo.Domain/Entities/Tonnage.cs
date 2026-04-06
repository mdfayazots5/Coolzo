namespace Coolzo.Domain.Entities;

public sealed class Tonnage : AuditableEntity
{
    public long TonnageId { get; set; }

    public string TonnageCode { get; set; } = string.Empty;

    public string TonnageName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<BookingLine> BookingLines { get; set; } = new List<BookingLine>();
}
