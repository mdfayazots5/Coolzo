namespace Coolzo.Application.Common.Models;

public sealed record TechnicianAvailabilitySnapshot(
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
    bool IsSkillMatched);
