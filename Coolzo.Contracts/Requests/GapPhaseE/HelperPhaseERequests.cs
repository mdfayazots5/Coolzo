namespace Coolzo.Contracts.Requests.GapPhaseE;

public sealed record CreateHelperProfileRequest(
    long UserId,
    string HelperCode,
    string HelperName,
    string MobileNo,
    bool ActiveFlag);

public sealed record AssignHelperToJobRequest(
    long TechnicianId,
    long ServiceRequestId,
    long? JobCardId,
    string? AssignmentRemarks);

public sealed record ReleaseHelperAssignmentRequest(
    string Remarks);

public sealed record CheckInHelperAttendanceRequest(
    string? LocationText);

public sealed record CheckOutHelperAttendanceRequest(
    string? LocationText);

public sealed record SaveHelperTaskResponseRequest(
    string ResponseStatus,
    string? ResponseRemarks);

public sealed record UploadHelperTaskPhotoRequest(
    string FileName,
    string ContentType,
    string Base64Content,
    string? ResponseRemarks);
