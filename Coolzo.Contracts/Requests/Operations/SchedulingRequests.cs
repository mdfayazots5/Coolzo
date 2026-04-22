namespace Coolzo.Contracts.Requests.Operations;

public sealed record ScheduleAssignServiceRequest(
    long ServiceRequestId,
    long TechnicianId,
    long SlotAvailabilityId,
    string? Remarks);

public sealed record ScheduleReassignServiceRequest(
    long ServiceRequestId,
    long TechnicianId,
    long SlotAvailabilityId,
    string? Remarks);

public sealed record ScheduleAmcBulkAssignRequest(
    long TechnicianId,
    IReadOnlyCollection<ScheduleAmcBulkAssignVisitRequest> Visits,
    string? Remarks);

public sealed record ScheduleAmcBulkAssignVisitRequest(
    long AmcVisitScheduleId,
    long SlotAvailabilityId);

public sealed record ScheduleUpdateSlotRequest(
    bool IsBlocked,
    int? AvailableCapacity);

public sealed record ScheduleUpdateTechnicianShiftsRequest(
    long TechnicianId,
    IReadOnlyCollection<ScheduleShiftDayRequest> Days);

public sealed record ScheduleShiftDayRequest(
    int DayOfWeekNumber,
    bool IsOffDuty,
    string? ShiftStartTime,
    string? ShiftEndTime,
    string? BreakStartTime,
    string? BreakEndTime);
