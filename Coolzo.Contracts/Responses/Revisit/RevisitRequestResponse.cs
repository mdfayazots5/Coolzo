namespace Coolzo.Contracts.Responses.Revisit;

public sealed record RevisitRequestResponse(
    long RevisitRequestId,
    long BookingId,
    string BookingReference,
    long CustomerId,
    string CustomerName,
    long OriginalJobCardId,
    string OriginalJobCardNumber,
    string RevisitType,
    string CurrentStatus,
    DateTime RequestedDateUtc,
    DateTime? PreferredVisitDateUtc,
    string IssueSummary,
    string RequestRemarks,
    decimal ChargeAmount,
    long? CustomerAmcId,
    long? WarrantyClaimId);
