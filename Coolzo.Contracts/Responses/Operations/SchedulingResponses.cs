namespace Coolzo.Contracts.Responses.Operations;

public sealed record SchedulingBoardResponse(
    DateOnly DateFrom,
    DateOnly DateTo,
    DateTime GeneratedOnUtc,
    IReadOnlyCollection<SchedulingTimeSlotResponse> TimeSlots,
    IReadOnlyCollection<SchedulingTechnicianResponse> Technicians,
    IReadOnlyCollection<SchedulingBoardJobResponse> Jobs,
    IReadOnlyCollection<SchedulingBoardJobResponse> UnassignedJobs);

public sealed record SchedulingTimeSlotResponse(
    string SlotKey,
    string SlotLabel,
    string StartTime,
    string EndTime);

public sealed record SchedulingTechnicianResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    string AvailabilityStatus,
    string? BaseZoneName,
    IReadOnlyCollection<string> Zones,
    IReadOnlyCollection<string> Skills,
    decimal AverageRating,
    int TodayJobCount,
    string? NextFreeSlot,
    IReadOnlyCollection<SchedulingShiftDayResponse> WeeklyShifts);

public sealed record SchedulingBoardJobResponse(
    long ServiceRequestId,
    string ServiceRequestNumber,
    long BookingId,
    long ZoneId,
    string ZoneName,
    string CustomerName,
    string MobileNumber,
    string AddressSummary,
    string ServiceName,
    string? AcTypeName,
    string? BrandName,
    string Priority,
    string CurrentStatus,
    long SlotAvailabilityId,
    DateOnly SlotDate,
    string SlotLabel,
    string StartTime,
    string EndTime,
    int DurationMinutes,
    long? TechnicianId,
    string? TechnicianName,
    decimal EstimatedPrice);

public sealed record SchedulingConflictResponse(
    string ConflictType,
    string Severity,
    string Message,
    string? SuggestedResolution,
    long? RelatedServiceRequestId,
    string? RelatedServiceRequestNumber);

public sealed record SchedulingSlotResponse(
    long SlotAvailabilityId,
    long ZoneId,
    string ZoneName,
    DateOnly SlotDate,
    string SlotLabel,
    string StartTime,
    string EndTime,
    int AvailableCapacity,
    int ReservedCapacity,
    bool IsBlocked,
    bool IsAvailable);

public sealed record SchedulingShiftResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    IReadOnlyCollection<SchedulingShiftDayResponse> Days);

public sealed record SchedulingShiftDayResponse(
    int DayOfWeekNumber,
    string DayName,
    bool IsOffDuty,
    string? ShiftStartTime,
    string? ShiftEndTime,
    string? BreakStartTime,
    string? BreakEndTime);

public sealed record SchedulingAmcAutoVisitResponse(
    long AmcVisitScheduleId,
    long CustomerAmcId,
    int VisitNumber,
    DateOnly ScheduledDate,
    string CurrentStatus,
    long CustomerId,
    string CustomerName,
    string MobileNumber,
    long CustomerAddressId,
    long ZoneId,
    string ZoneName,
    string AddressSummary,
    long ServiceId,
    string ServiceName,
    string? AcTypeName,
    string? BrandName,
    string JobCardNumber,
    string? OriginServiceRequestNumber,
    string AmcPlanName,
    long? LinkedServiceRequestId,
    string? LinkedServiceRequestNumber);

public sealed record SchedulingDaySheetResponse(
    DateOnly ScheduleDate,
    DateTime GeneratedOnUtc,
    IReadOnlyCollection<SchedulingDaySheetTechnicianResponse> Technicians);

public sealed record SchedulingDaySheetTechnicianResponse(
    long TechnicianId,
    string TechnicianCode,
    string TechnicianName,
    string? BaseZoneName,
    IReadOnlyCollection<SchedulingDaySheetItemResponse> Itinerary);

public sealed record SchedulingDaySheetItemResponse(
    long ServiceRequestId,
    string ServiceRequestNumber,
    string CustomerName,
    string MobileNumber,
    string AddressSummary,
    string ServiceName,
    string SlotLabel,
    string StartTime,
    string EndTime,
    string CurrentStatus,
    string Priority,
    string ZoneName);
