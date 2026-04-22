namespace Coolzo.Contracts.Responses.Technician;

public sealed record TechnicianSkillResponse(
    long TechnicianSkillId,
    string SkillCode,
    string SkillName,
    string SkillCategory,
    DateTime? CertifiedOnUtc);

public sealed record TechnicianZoneResponse(
    long TechnicianZoneId,
    long ZoneId,
    string ZoneName,
    bool IsPrimaryZone);

public sealed record TechnicianListItemResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    string MobileNumber,
    string EmailAddress,
    bool IsActive,
    string AvailabilityStatus,
    string? CurrentServiceRequestNumber,
    string? BaseZoneName,
    IReadOnlyCollection<string> Zones,
    IReadOnlyCollection<TechnicianSkillResponse> Skills,
    decimal AverageRating,
    int TodayJobCount,
    decimal SlaCompliancePercent,
    string? NextFreeSlot);

public sealed record TechnicianDetailResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    string MobileNumber,
    string EmailAddress,
    long? BaseZoneId,
    string? BaseZoneName,
    bool IsActive,
    int MaxDailyAssignments,
    string AvailabilityStatus,
    string? CurrentServiceRequestNumber,
    IReadOnlyCollection<TechnicianZoneResponse> Zones,
    IReadOnlyCollection<TechnicianSkillResponse> Skills,
    string OnboardingStatus,
    IReadOnlyCollection<string> PendingEligibilityItems,
    int UploadedDocumentCount,
    int VerifiedDocumentCount,
    string LatestAssessmentResult,
    int CompletedTrainingCount);

public sealed record TechnicianPerformanceTrendResponse(
    string Label,
    int JobsAssigned,
    int JobsCompleted,
    decimal SlaCompliancePercent);

public sealed record TechnicianPerformanceResponse(
    decimal AverageRating,
    int TotalJobs,
    int CompletedJobs,
    decimal SlaCompliancePercent,
    decimal RevisitRatePercent,
    decimal RevenueGenerated,
    decimal TeamAverageSlaCompliancePercent,
    IReadOnlyCollection<TechnicianPerformanceTrendResponse> Trends);

public sealed record TechnicianAttendanceResponse(
    long TechnicianAttendanceId,
    DateOnly AttendanceDate,
    string AttendanceStatus,
    DateTime? CheckInOnUtc,
    DateTime? CheckOutOnUtc,
    string LocationText,
    string LeaveReason,
    long? ReviewedByUserId,
    DateTime? ReviewedOnUtc);

public sealed record TechnicianGpsLogResponse(
    long TechnicianGpsLogId,
    DateTime TrackedOnUtc,
    decimal Latitude,
    decimal Longitude,
    string TrackingSource,
    string LocationText,
    long? ServiceRequestId);
