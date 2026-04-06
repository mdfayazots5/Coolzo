namespace Coolzo.Application.Common.Models;

public sealed record TechnicianMonitoringView(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    int TodayAssignedJobsCount,
    int ActiveJobsCount,
    string? CurrentActiveJobNumber,
    string? CurrentActiveStatus);
