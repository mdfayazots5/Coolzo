namespace Coolzo.Contracts.Responses.Operations;

public sealed record TechnicianMonitoringItemResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    int TodayAssignedJobsCount,
    int ActiveJobsCount,
    string? CurrentActiveJobNumber,
    string? CurrentActiveStatus);
