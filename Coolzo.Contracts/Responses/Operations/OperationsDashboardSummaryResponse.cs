namespace Coolzo.Contracts.Responses.Operations;

public sealed record OperationsDashboardSummaryResponse(
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
    IReadOnlyCollection<TechnicianMonitoringItemResponse> TechnicianMonitoring);
