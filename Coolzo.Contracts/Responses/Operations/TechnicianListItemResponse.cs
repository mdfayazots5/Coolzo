namespace Coolzo.Contracts.Responses.Operations;

public sealed record TechnicianListItemResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    string MobileNumber,
    string EmailAddress,
    string? BaseZoneName,
    bool IsActive,
    int MaxDailyAssignments,
    int ActiveAssignments);
