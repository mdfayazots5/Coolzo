namespace Coolzo.Contracts.Requests.GapPhaseD;

public sealed record CreateCustomerCancellationRequest(
    long? BookingId,
    long? ServiceRequestId,
    string CancellationReasonCode,
    string CancellationReasonText);

public sealed record CreateAdminCancellationRequest(
    long? BookingId,
    long? ServiceRequestId,
    string CancellationSource,
    string CancellationReasonCode,
    string CancellationReasonText,
    bool ForceOverride,
    string? OverrideReason);

public sealed record CreateRefundRequestCommandRequest(
    long CancellationRecordId,
    long? InvoiceId,
    decimal RefundAmount,
    string RefundMethod,
    string RefundReason);

public sealed record ApproveRefundRequestDecisionRequest(
    decimal? ApprovedAmount,
    string Remarks);

public sealed record RejectRefundRequestDecisionRequest(
    string Remarks);

public sealed record UpdateRefundStatusRequest(
    string RefundStatus,
    string Remarks);

public sealed record MarkCustomerAbsentRequest(
    string AbsentReasonCode,
    string AbsentReasonText,
    int AttemptCount,
    string ContactAttemptLog);

public sealed record ResolveCustomerAbsentRequest(
    string Remarks);

public sealed record CancelCustomerAbsentRequest(
    string CancellationReasonCode,
    string CancellationReasonText,
    string Remarks);
