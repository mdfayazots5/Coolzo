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

    public ICollection<TechnicianSkill> Skills { get; set; } = new List<TechnicianSkill>();

    public ICollection<TechnicianZone> Zones { get; set; } = new List<TechnicianZone>();

    public ICollection<TechnicianAvailability> TechnicianAvailabilities { get; set; } = new List<TechnicianAvailability>();

    public ICollection<TechnicianShift> Shifts { get; set; } = new List<TechnicianShift>();

    public ICollection<TechnicianAttendance> Attendances { get; set; } = new List<TechnicianAttendance>();

    public ICollection<TechnicianGpsLog> GpsLogs { get; set; } = new List<TechnicianGpsLog>();

    public ICollection<TechnicianPerformanceSummary> PerformanceSummaries { get; set; } = new List<TechnicianPerformanceSummary>();

    public ICollection<TechnicianVanStock> VanStocks { get; set; } = new List<TechnicianVanStock>();

    public ICollection<AssignmentLog> PreviousAssignmentLogs { get; set; } = new List<AssignmentLog>();

    public ICollection<AssignmentLog> CurrentAssignmentLogs { get; set; } = new List<AssignmentLog>();

    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

    public ICollection<JobPartConsumption> JobPartConsumptions { get; set; } = new List<JobPartConsumption>();

    public ICollection<JobReport> JobReports { get; set; } = new List<JobReport>();

    public ICollection<JobPhoto> JobPhotos { get; set; } = new List<JobPhoto>();

    public ICollection<CustomerSignature> CustomerSignatures { get; set; } = new List<CustomerSignature>();

    public ICollection<PartsRequest> PartsRequests { get; set; } = new List<PartsRequest>();
}
