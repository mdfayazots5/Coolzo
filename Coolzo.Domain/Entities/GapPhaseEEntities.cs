namespace Coolzo.Domain.Entities;

public sealed class TechnicianActivationLog : AuditableEntity
{
    public long TechnicianActivationLogId { get; set; }

    public long TechnicianId { get; set; }

    public string ActivationAction { get; set; } = string.Empty;

    public string ActivationReason { get; set; } = string.Empty;

    public long? ActivatedByUserId { get; set; }

    public DateTime ActivatedOnUtc { get; set; }

    public string EligibilitySnapshot { get; set; } = string.Empty;

    public Technician? Technician { get; set; }
}

public sealed class HelperProfile : AuditableEntity
{
    public long HelperProfileId { get; set; }

    public long UserId { get; set; }

    public string HelperCode { get; set; } = string.Empty;

    public string HelperName { get; set; } = string.Empty;

    public string MobileNo { get; set; } = string.Empty;

    public bool ActiveFlag { get; set; } = true;

    public User? User { get; set; }

    public ICollection<HelperAssignment> Assignments { get; set; } = new List<HelperAssignment>();

    public ICollection<HelperAttendance> Attendances { get; set; } = new List<HelperAttendance>();
}

public sealed class HelperAssignment : AuditableEntity
{
    public long HelperAssignmentId { get; set; }

    public long HelperProfileId { get; set; }

    public long TechnicianId { get; set; }

    public long? ServiceRequestId { get; set; }

    public long? JobCardId { get; set; }

    public string AssignmentStatus { get; set; } = string.Empty;

    public string AssignmentRemarks { get; set; } = string.Empty;

    public DateTime AssignedOnUtc { get; set; }

    public DateTime? ReleasedOnUtc { get; set; }

    public HelperProfile? HelperProfile { get; set; }

    public Technician? Technician { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }

    public JobCard? JobCard { get; set; }

    public ICollection<HelperTaskResponse> TaskResponses { get; set; } = new List<HelperTaskResponse>();
}

public sealed class HelperTaskChecklist : AuditableEntity
{
    public long HelperTaskChecklistId { get; set; }

    public long? ServiceTypeId { get; set; }

    public string TaskName { get; set; } = string.Empty;

    public string TaskDescription { get; set; } = string.Empty;

    public bool MandatoryFlag { get; set; }

    public ICollection<HelperTaskResponse> Responses { get; set; } = new List<HelperTaskResponse>();
}

public sealed class HelperTaskResponse : AuditableEntity
{
    public long HelperTaskResponseId { get; set; }

    public long HelperAssignmentId { get; set; }

    public long HelperTaskChecklistId { get; set; }

    public string ResponseStatus { get; set; } = string.Empty;

    public string ResponseRemarks { get; set; } = string.Empty;

    public string ResponsePhotoUrl { get; set; } = string.Empty;

    public DateTime RespondedOnUtc { get; set; }

    public HelperAssignment? HelperAssignment { get; set; }

    public HelperTaskChecklist? HelperTaskChecklist { get; set; }
}

public sealed class HelperAttendance : AuditableEntity
{
    public long HelperAttendanceId { get; set; }

    public long HelperProfileId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public DateTime? CheckInOnUtc { get; set; }

    public DateTime? CheckOutOnUtc { get; set; }

    public string AttendanceStatus { get; set; } = string.Empty;

    public string LocationText { get; set; } = string.Empty;

    public HelperProfile? HelperProfile { get; set; }
}
