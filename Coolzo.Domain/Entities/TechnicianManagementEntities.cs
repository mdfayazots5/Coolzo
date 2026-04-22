namespace Coolzo.Domain.Entities;

public sealed class TechnicianSkill : AuditableEntity
{
    public long TechnicianSkillId { get; set; }

    public long TechnicianId { get; set; }

    public string SkillCode { get; set; } = string.Empty;

    public string SkillName { get; set; } = string.Empty;

    public string SkillCategory { get; set; } = "special";

    public DateTime? CertifiedOnUtc { get; set; }

    public Technician? Technician { get; set; }
}

public sealed class TechnicianZone : AuditableEntity
{
    public long TechnicianZoneId { get; set; }

    public long TechnicianId { get; set; }

    public long ZoneId { get; set; }

    public bool IsPrimaryZone { get; set; }

    public Technician? Technician { get; set; }

    public Zone? Zone { get; set; }
}

public sealed class TechnicianAttendance : AuditableEntity
{
    public long TechnicianAttendanceId { get; set; }

    public long TechnicianId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public DateTime? CheckInOnUtc { get; set; }

    public DateTime? CheckOutOnUtc { get; set; }

    public string AttendanceStatus { get; set; } = "Pending";

    public string LocationText { get; set; } = string.Empty;

    public string LeaveReason { get; set; } = string.Empty;

    public long? ReviewedByUserId { get; set; }

    public DateTime? ReviewedOnUtc { get; set; }

    public Technician? Technician { get; set; }

    public User? ReviewedByUser { get; set; }
}

public sealed class TechnicianGpsLog : AuditableEntity
{
    public long TechnicianGpsLogId { get; set; }

    public long TechnicianId { get; set; }

    public long? ServiceRequestId { get; set; }

    public DateTime TrackedOnUtc { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string TrackingSource { get; set; } = string.Empty;

    public string LocationText { get; set; } = string.Empty;

    public Technician? Technician { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }
}

public sealed class TechnicianPerformanceSummary : AuditableEntity
{
    public long TechnicianPerformanceSummaryId { get; set; }

    public long TechnicianId { get; set; }

    public DateOnly SummaryDate { get; set; }

    public decimal AverageRating { get; set; }

    public int TotalJobs { get; set; }

    public int CompletedJobs { get; set; }

    public decimal SlaCompliancePercent { get; set; }

    public decimal RevisitRatePercent { get; set; }

    public decimal RevenueGenerated { get; set; }

    public Technician? Technician { get; set; }
}
