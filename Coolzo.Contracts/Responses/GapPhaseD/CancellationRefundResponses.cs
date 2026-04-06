namespace Coolzo.Contracts.Responses.GapPhaseD;

public sealed record CancellationOptionsResponse(
    long? BookingId,
    long? ServiceRequestId,
    string PolicyCode,
    string PolicyName,
    string PolicyDescription,
    int TimeToSlotMinutes,
    decimal PaidAmount,
    decimal CancellationFee,
    decimal RefundEligibleAmount,
    bool ApprovalRequired,
    bool CanCustomerCancel,
    string CustomerDenialReason,
    DateTime ScheduledStartUtc,
    bool IsTechnicianDispatched);

public sealed record CancellationListItemResponse(
    long CancellationRecordId,
    long? BookingId,
    long? ServiceRequestId,
    string ReferenceNumber,
    string CancellationStatus,
    string CancellationSource,
    string CancellationReasonCode,
    decimal CancellationFee,
    decimal RefundEligibleAmount,
    string CancelledByRole,
    DateTime DateCreated,
    long? RefundRequestId,
    string? RefundStatus);

public sealed record CancellationDetailResponse(
    long CancellationRecordId,
    long? BookingId,
    long? ServiceRequestId,
    long? CancelledByUserId,
    string CancelledByRole,
    string CancellationSource,
    string CancellationReasonCode,
    string CancellationReasonText,
    int TimeToSlotMinutes,
    decimal CancellationFee,
    decimal RefundEligibleAmount,
    string CancellationStatus,
    string PolicyCode,
    string PolicyDescription,
    bool ApprovalRequired,
    DateTime DateCreated,
    long? RefundRequestId,
    string? RefundStatus);

public sealed record RefundApprovalHistoryResponse(
    long RefundApprovalId,
    int ApprovalLevel,
    long ApproverUserId,
    string ApprovalStatus,
    string ApprovalRemarks,
    DateTime? ApprovedOn);

public sealed record RefundStatusHistoryResponse(
    long RefundStatusHistoryId,
    string FromStatus,
    string ToStatus,
    long ChangedByUserId,
    DateTime ChangedOn,
    string Remarks);

public sealed record RefundListItemResponse(
    long RefundRequestId,
    string RefundRequestNo,
    long? CancellationRecordId,
    long? InvoiceId,
    long? PaymentTransactionId,
    decimal RefundAmount,
    string RefundMethod,
    string RefundStatus,
    bool ApprovalRequired,
    DateTime DateCreated,
    DateTime? ProcessedOn);

public sealed record RefundDetailResponse(
    long RefundRequestId,
    string RefundRequestNo,
    long? CancellationRecordId,
    long? InvoiceId,
    long? PaymentTransactionId,
    decimal RefundAmount,
    decimal RequestedAmount,
    decimal ApprovedAmount,
    decimal MaxAllowedAmount,
    string RefundMethod,
    string RefundReason,
    string RefundStatus,
    bool ApprovalRequired,
    long? ApprovedByUserId,
    DateTime? ApprovedDateUtc,
    DateTime? ProcessedOn,
    IReadOnlyCollection<RefundApprovalHistoryResponse> Approvals,
    IReadOnlyCollection<RefundStatusHistoryResponse> StatusHistory);

public sealed record CustomerRefundStatusResponse(
    long RefundRequestId,
    string RefundRequestNo,
    decimal RefundAmount,
    string RefundMethod,
    string RefundStatus,
    DateTime DateCreated,
    DateTime? ProcessedOn);

public sealed record CustomerAbsentDetailResponse(
    long CustomerAbsentRecordId,
    long ServiceRequestId,
    string ServiceRequestNumber,
    long BookingId,
    string BookingReference,
    long TechnicianId,
    string TechnicianName,
    DateTime MarkedOn,
    int AttemptCount,
    string ContactAttemptLog,
    string AbsentReasonCode,
    string AbsentReasonText,
    string CustomerAbsentStatus,
    string ServiceRequestStatus,
    string ResolutionRemarks,
    DateTime? ResolvedOn);
