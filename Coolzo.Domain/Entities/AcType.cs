namespace Coolzo.Domain.Entities;

public sealed class AcType : AuditableEntity
{
    public long AcTypeId { get; set; }

    public string AcTypeCode { get; set; } = string.Empty;

    public string AcTypeName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<BookingLine> BookingLines { get; set; } = new List<BookingLine>();
}
