namespace Coolzo.Domain.Entities;

public sealed class WarrantyRule : AuditableEntity
{
    public long WarrantyRuleId { get; set; }

    public string RuleName { get; set; } = string.Empty;

    public long? ServiceId { get; set; }

    public long? AcTypeId { get; set; }

    public long? BrandId { get; set; }

    public int WarrantyDurationDays { get; set; }

    public string CoverageDescription { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Service? Service { get; set; }

    public AcType? AcType { get; set; }

    public Brand? Brand { get; set; }

    public ICollection<WarrantyClaim> WarrantyClaims { get; set; } = new List<WarrantyClaim>();
}
