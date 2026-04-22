using Coolzo.Contracts.Responses.Billing;
using Coolzo.Contracts.Responses.TechnicianJobs;

namespace Coolzo.Contracts.Responses.FieldWorkflow;

public sealed record FieldJobDetailResponse(
    TechnicianJobDetailResponse Job,
    double? CustomerLatitude,
    double? CustomerLongitude,
    FieldJobReportResponse? LatestReport,
    IReadOnlyCollection<FieldJobPhotoResponse> Photos,
    FieldCustomerSignatureResponse? Signature,
    IReadOnlyCollection<FieldPartsRequestResponse> PartsRequests,
    QuotationDetailResponse? Quotation,
    InvoiceDetailResponse? Invoice,
    IReadOnlyCollection<PaymentTransactionResponse> Payments);

public sealed record FieldJobReportResponse(
    long JobReportId,
    long ServiceRequestId,
    long JobCardId,
    long TechnicianId,
    IReadOnlyCollection<string> IssuesIdentified,
    string EquipmentCondition,
    string ActionTaken,
    string Recommendation,
    string Observations,
    DateTime SubmittedAtUtc,
    bool IsQualityReviewed,
    decimal QualityScore);

public sealed record FieldJobPhotoResponse(
    long JobPhotoId,
    long ServiceRequestId,
    long JobCardId,
    string PhotoType,
    string FileName,
    string ContentType,
    string StorageUrl,
    string UploadedBy,
    DateTime UploadedAtUtc,
    string PhotoRemarks);

public sealed record FieldCustomerSignatureResponse(
    long CustomerSignatureId,
    long ServiceRequestId,
    long JobCardId,
    string CustomerName,
    string SignatureDataUrl,
    DateTime SignedAtUtc,
    string CapturedBy,
    string SignatureRemarks);

public sealed record FieldPartsRequestItemResponse(
    long PartsRequestItemId,
    string PartCode,
    string PartName,
    decimal QuantityRequested,
    decimal QuantityApproved,
    string CurrentStatus,
    string ItemRemarks);

public sealed record FieldPartsRequestResponse(
    long PartsRequestId,
    long ServiceRequestId,
    long JobCardId,
    long TechnicianId,
    string Urgency,
    string CurrentStatus,
    string Notes,
    DateTime SubmittedAtUtc,
    DateTime? ProcessedAtUtc,
    IReadOnlyCollection<FieldPartsRequestItemResponse> Items);

public sealed record FieldArrivalValidationResponse(
    bool OverrideRequired,
    double DistanceMeters,
    string Message,
    FieldJobDetailResponse? Job);
