namespace Coolzo.Contracts.Requests.Technician;

public sealed record TechnicianSkillRequest(
    string SkillCode,
    string SkillName,
    string SkillCategory,
    DateTime? CertifiedOnUtc);

public sealed record CreateTechnicianRequest(
    string TechnicianName,
    string MobileNumber,
    string? EmailAddress,
    long? BaseZoneId,
    int MaxDailyAssignments,
    IReadOnlyCollection<TechnicianSkillRequest>? Skills,
    IReadOnlyCollection<long>? ZoneIds);

public sealed record UpdateTechnicianRequest(
    string TechnicianName,
    string MobileNumber,
    string? EmailAddress,
    long? BaseZoneId,
    int MaxDailyAssignments,
    bool IsActive);

public sealed record UpdateTechnicianSkillsRequest(
    IReadOnlyCollection<TechnicianSkillRequest> Skills);

public sealed record UpdateTechnicianZonesRequest(
    IReadOnlyCollection<long> ZoneIds,
    long? PrimaryZoneId);

public sealed record CreateTechnicianLeaveRequest(
    DateOnly LeaveDate,
    string? LeaveReason);

public sealed record ReviewTechnicianLeaveRequest(
    string Decision,
    string? Remarks);
