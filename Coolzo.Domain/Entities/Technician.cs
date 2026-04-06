namespace Coolzo.Domain.Entities;

public sealed class Technician : AuditableEntity
{
    public long TechnicianId { get; set; }

    public long? UserId { get; set; }

    public string TechnicianCode { get; set; } = string.Empty;

    public string TechnicianName { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string EmailAddress { get; set; } = string.Empty;

    public long? BaseZoneId { get; set; }

    public bool IsActive { get; set; } = true;

    public int MaxDailyAssignments { get; set; } = 4;

    public User? User { get; set; }

    public Zone? BaseZone { get; set; }

    public ICollection<ServiceRequestAssignment> ServiceRequestAssignments { get; set; } = new List<ServiceRequestAssignment>();

    public ICollection<TechnicianSkillMapping> SkillMappings { get; set; } = new List<TechnicianSkillMapping>();

    public ICollection<TechnicianAvailability> TechnicianAvailabilities { get; set; } = new List<TechnicianAvailability>();

    public ICollection<TechnicianVanStock> VanStocks { get; set; } = new List<TechnicianVanStock>();

    public ICollection<AssignmentLog> PreviousAssignmentLogs { get; set; } = new List<AssignmentLog>();

    public ICollection<AssignmentLog> CurrentAssignmentLogs { get; set; } = new List<AssignmentLog>();

    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

    public ICollection<JobPartConsumption> JobPartConsumptions { get; set; } = new List<JobPartConsumption>();
}
