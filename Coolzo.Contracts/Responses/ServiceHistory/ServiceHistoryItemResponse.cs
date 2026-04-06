namespace Coolzo.Contracts.Responses.ServiceHistory;

public sealed record ServiceHistoryItemResponse(
    string HistoryType,
    string ReferenceNumber,
    string Title,
    string Status,
    DateTime EventDateUtc,
    string Detail,
    decimal? Amount,
    long? BookingId,
    long? ServiceRequestId,
    long? JobCardId,
    long? InvoiceId,
    long? CustomerAmcId,
    long? RevisitRequestId);
