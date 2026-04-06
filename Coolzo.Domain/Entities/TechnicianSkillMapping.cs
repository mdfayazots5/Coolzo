namespace Coolzo.Domain.Entities;

public sealed class TechnicianSkillMapping : AuditableEntity
{
    public long TechnicianSkillMappingId { get; set; }

    public long TechnicianId { get; set; }

    public long ServiceId { get; set; }

    public long? AcTypeId { get; set; }

    public bool IsPrimarySkill { get; set; }

    public Technician? Technician { get; set; }

    public Service? Service { get; set; }

    public AcType? AcType { get; set; }
}
