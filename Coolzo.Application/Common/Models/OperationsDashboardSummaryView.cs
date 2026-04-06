namespace Coolzo.Application.Common.Models;

public sealed record OperationsDashboardSummaryView(
    int TotalBookings,
    int TotalServiceRequests,
    int AssignedServiceRequests,
    int UnassignedServiceRequests,
    int EnRouteCount,
    int ReachedCount,
    int WorkStartedCount,
    int WorkInProgressCount,
    int SubmittedForClosureCount,
    int ActiveTechnicianCount,
    IReadOnlyCollection<TechnicianMonitoringView> TechnicianMonitoring);
