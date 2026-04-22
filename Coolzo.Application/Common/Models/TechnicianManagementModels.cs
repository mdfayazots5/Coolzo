namespace Coolzo.Application.Common.Models;

public sealed record TechnicianPerformanceTrendPointMetric(
    DateOnly MetricDate,
    int JobsAssigned,
    int JobsCompleted,
    decimal SlaCompliancePercent);

public sealed record TechnicianPerformanceMetricsSnapshot(
    decimal AverageRating,
    int TotalJobs,
    int CompletedJobs,
    decimal SlaCompliancePercent,
    decimal RevisitRatePercent,
    decimal RevenueGenerated,
    IReadOnlyCollection<TechnicianPerformanceTrendPointMetric> TrendPoints);
