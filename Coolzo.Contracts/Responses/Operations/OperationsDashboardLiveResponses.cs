namespace Coolzo.Contracts.Responses.Operations;

public sealed record OperationsDashboardResponse(
    int TotalJobs,
    int PendingQueueCount,
    int AssignedCount,
    int InProgressCount,
    int CompletedCount,
    int ActiveTechnicianCount,
    int AtRiskAlertCount,
    int BreachedAlertCount,
    decimal SlaCompliancePercent,
    DateTime LastUpdatedUtc);

public sealed record OperationsPendingQueueItemResponse(
    long ServiceRequestId,
    string ServiceRequestNumber,
    string CustomerName,
    string MobileNumber,
    string ZoneName,
    string ServiceName,
    string AddressSummary,
    DateOnly SlotDate,
    string SlotLabel,
    string Priority,
    string CurrentStatus,
    decimal EstimatedPrice,
    DateTime CreatedOnUtc);

public sealed record OperationsTechnicianStatusItemResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    string MobileNumber,
    string EmailAddress,
    string AvailabilityStatus,
    string? CurrentServiceRequestNumber,
    string? BaseZoneName,
    IReadOnlyCollection<string> Zones,
    IReadOnlyCollection<string> Skills,
    decimal AverageRating,
    int TodayJobCount,
    string? NextFreeSlot);

public sealed record OperationsSlaAlertItemResponse(
    long SystemAlertId,
    long? ServiceRequestId,
    string? ServiceRequestNumber,
    string CustomerName,
    string ZoneName,
    string ServiceName,
    string Priority,
    string AlertType,
    string AlertState,
    string Severity,
    DateTime? SlaDueDateUtc,
    int? MinutesFromDue,
    int EscalationLevel,
    string AlertMessage,
    string? AssignedTechnicianName);

public sealed record OperationsZoneWorkloadItemResponse(
    string ZoneName,
    int TotalCount,
    int PendingCount,
    int AssignedCount,
    int InProgressCount,
    int CompletedCount,
    int EmergencyCount,
    int BreachedAlertCount,
    int ActiveTechnicianCount);

public sealed record OperationsDaySummaryResponse(
    DateOnly SummaryDate,
    int TotalJobs,
    int PendingQueueCount,
    int AssignedCount,
    int InProgressCount,
    int CompletedCount,
    int SubmittedForClosureCount,
    int CarryForwardCount,
    int EmergencyCount,
    int EscalatedCount,
    int AtRiskAlertCount,
    int BreachedAlertCount,
    int ActiveTechnicianCount,
    decimal SlaCompliancePercent,
    IReadOnlyCollection<OperationsZoneWorkloadItemResponse> ZoneWorkload,
    DateTime GeneratedOnUtc);

public sealed record OperationsMapCoordinateResponse(
    double Latitude,
    double Longitude,
    DateTime TrackedOnUtc);

public sealed record OperationsLiveMapTechnicianPinResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    string AvailabilityStatus,
    string? CurrentServiceRequestNumber,
    string? BaseZoneName,
    double Latitude,
    double Longitude,
    DateTime TrackedOnUtc,
    IReadOnlyCollection<OperationsMapCoordinateResponse> Breadcrumbs);

public sealed record OperationsLiveMapServiceRequestPinResponse(
    long ServiceRequestId,
    string ServiceRequestNumber,
    string CustomerName,
    string ServiceName,
    string CurrentStatus,
    string Priority,
    string ZoneName,
    string AddressSummary,
    string? AssignedTechnicianName,
    double Latitude,
    double Longitude);

public sealed record OperationsLiveMapResponse(
    DateTime GeneratedOnUtc,
    IReadOnlyCollection<OperationsLiveMapTechnicianPinResponse> TechnicianPins,
    IReadOnlyCollection<OperationsLiveMapServiceRequestPinResponse> ServiceRequestPins);
