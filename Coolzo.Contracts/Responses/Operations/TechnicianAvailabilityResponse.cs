namespace Coolzo.Contracts.Responses.Operations;

public sealed record TechnicianAvailabilityResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    string MobileNumber,
    string EmailAddress,
    string? BaseZoneName,
    DateOnly AvailableDate,
    int AvailableSlotCount,
    int BookedAssignmentCount,
    int RemainingCapacity,
    bool IsAvailable,
    bool IsSkillMatched,
    string AvailabilityMessage);
