namespace Coolzo.Domain.Entities;

public abstract class AuditableEntity
{
    public int CompanyId { get; set; } = 1;

    public int SiteId { get; set; } = 1;

    public int BranchId { get; set; } = 1;

    public int? DepartmentId { get; set; }

    public string? Tag { get; set; }

    public string? Comments { get; set; }

    public bool DisplayOnWeb { get; set; } = true;

    public bool IsPublished { get; set; } = true;

    public DateTime? DatePublished { get; set; }

    public string? PublishedBy { get; set; }

    public int SortOrder { get; set; }

    public string IPAddress { get; set; } = "127.0.0.1";

    public string CreatedBy { get; set; } = "System";

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public string? UpdatedBy { get; set; }

    public DateTime? LastUpdated { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DateDeleted { get; set; }

    public bool IsDeleted { get; set; }
}
