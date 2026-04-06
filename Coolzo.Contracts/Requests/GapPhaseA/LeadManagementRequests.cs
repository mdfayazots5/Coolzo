namespace Coolzo.Contracts.Requests.GapPhaseA;

public sealed record CreateLeadRequest(
    string CustomerName,
    string MobileNumber,
    string? EmailAddress,
    string SourceChannel,
    string? AddressLine1,
    string? AddressLine2,
    string? CityName,
    string? Pincode,
    long? ServiceId,
    long? AcTypeId,
    long? TonnageId,
    long? BrandId,
    long? SlotAvailabilityId,
    string? InquiryNotes);

public sealed record AssignLeadRequest(
    long AssignedUserId,
    string? Remarks);

public sealed record UpdateLeadStatusRequest(
    string LeadStatus,
    string? Remarks,
    string? LostReason);

public sealed record ConvertLeadToBookingRequest(
    long? ServiceId,
    long? AcTypeId,
    long? TonnageId,
    long? BrandId,
    long? SlotAvailabilityId,
    string? AddressLine1,
    string? AddressLine2,
    string? CityName,
    string? Pincode,
    string? InquiryNotes);

public sealed record ConvertLeadToServiceRequestRequest(
    long? ServiceId,
    long? AcTypeId,
    long? TonnageId,
    long? BrandId,
    long? SlotAvailabilityId,
    string? AddressLine1,
    string? AddressLine2,
    string? CityName,
    string? Pincode,
    string? InquiryNotes);

public sealed record AddLeadNoteRequest(
    string NoteText,
    bool IsInternal = true);
