namespace Coolzo.Domain.Entities;

public sealed class UnitOfMeasure : AuditableEntity
{
    public long UnitOfMeasureId { get; set; }

    public string UnitCode { get; set; } = string.Empty;

    public string UnitName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<Item> Items { get; set; } = new List<Item>();
}
