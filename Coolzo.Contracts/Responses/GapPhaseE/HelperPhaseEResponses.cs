namespace Coolzo.Contracts.Responses.GapPhaseE;

public sealed record HelperListItemResponse(
    long HelperProfileId,
    string HelperCode,
    string HelperName,
    string MobileNo,
    bool ActiveFlag,
    string? CurrentAssignmentStatus,
    string? PairedTechnicianName,
    string? ServiceRequestNumber);

public sealed record HelperAssignmentDetailResponse(
    long? HelperAssignmentId,
    string AssignmentStatus,
    long? TechnicianId,
    string? TechnicianName,
    long? ServiceRequestId,
    string? ServiceRequestNumber,
    long? JobCardId,
    string? JobCardNumber,
    string? CustomerName,
    string? ServiceName,
    string? AddressSummary,
    string AssignmentRemarks,
    DateTime? AssignedOnUtc,
    DateTime? ReleasedOnUtc);

public sealed record HelperAttendanceResponse(
    long HelperAttendanceId,
    DateOnly AttendanceDate,
    DateTime? CheckInOnUtc,
    DateTime? CheckOutOnUtc,
    string AttendanceStatus,
    string LocationText);

public sealed record HelperTaskChecklistResponse(
    long HelperTaskChecklistId,
    string TaskName,
    string TaskDescription,
    bool MandatoryFlag,
    int SortOrder,
    string ResponseStatus,
    string ResponseRemarks,
    string ResponsePhotoUrl,
    DateTime? RespondedOnUtc);

public sealed record HelperDetailResponse(
    long HelperProfileId,
    long UserId,
    string HelperCode,
    string HelperName,
    string MobileNo,
    bool ActiveFlag,
    HelperAssignmentDetailResponse CurrentAssignment,
    IReadOnlyCollection<HelperAttendanceResponse> AttendanceHistory,
    IReadOnlyCollection<HelperTaskChecklistResponse> TaskResponses);
