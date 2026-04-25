using Coolzo.Contracts.Requests.Billing;
using Coolzo.Contracts.Requests.FieldExecution;

namespace Coolzo.Contracts.Requests.FieldWorkflow;

public sealed record FieldJobStatusRequest(
    double? Latitude,
    double? Longitude,
    string? Remarks,
    string? OverrideReason);

public sealed record FieldJobProgressRequest(
    IReadOnlyCollection<SaveJobChecklistResponseItemRequest> Items,
    string? Remarks);

public sealed record FieldPartsRequestItemRequest(
    long PartId,
    decimal QuantityRequested,
    string? Remarks);

public sealed record FieldPartsRequestRequest(
    string Urgency,
    IReadOnlyCollection<FieldPartsRequestItemRequest> Items,
    string? Notes);

public sealed record FieldEstimateRequest(
    IReadOnlyCollection<QuotationLineRequest> Lines,
    decimal DiscountAmount,
    decimal TaxPercentage,
    string? Remarks);

public sealed record FieldJobReportRequest(
    string EquipmentCondition,
    IReadOnlyCollection<string> IssuesIdentified,
    string ActionTaken,
    string? Recommendation,
    string? Observations,
    string? IdempotencyKey);

public sealed record FieldJobPhotoUploadRequest(
    string PhotoType,
    string FileName,
    string ContentType,
    string Base64Content,
    string? Remarks);

public sealed record FieldJobSignatureRequest(
    string CustomerName,
    string SignatureBase64,
    string? Remarks);

public sealed record FieldJobPaymentRequest(
    decimal PaidAmount,
    string PaymentMethod,
    string? ReferenceNumber,
    string? Remarks,
    string? IdempotencyKey,
    string? GatewayTransactionId,
    string? Signature,
    decimal? ExpectedInvoiceAmount);

public sealed record FieldAttendanceRequest(
    string? LocationText,
    double? Latitude,
    double? Longitude);
