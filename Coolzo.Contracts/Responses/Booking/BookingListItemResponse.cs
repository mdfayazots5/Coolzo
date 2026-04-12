namespace Coolzo.Contracts.Responses.Booking;

public sealed record BookingListItemResponse(
    long BookingId,
    string BookingReference,
    string Status,
    string ServiceName,
    string CustomerName,
    string MobileNumber,
    DateOnly SlotDate,
    string SlotLabel,
    string SourceChannel,
    DateTime BookingDateUtc,
    string? OperationalStatus,
    string? AssignedTechnicianName,
    long? AssignedTechnicianId,
    string AddressSummary,
    decimal EstimatedPrice,
    bool IsEmergency,
    decimal EmergencySurchargeAmount,
    long? QuotationId,
    string? QuotationStatus,
    decimal? InvoiceGrandTotalAmount);
