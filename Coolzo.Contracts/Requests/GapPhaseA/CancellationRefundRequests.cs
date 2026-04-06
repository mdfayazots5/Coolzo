namespace Coolzo.Contracts.Requests.GapPhaseA;

public sealed record CancelServiceRequestRequest(
    string ReasonCode,
    string ReasonDescription,
    bool RequiresApproval);

public sealed record InitiateRefundRequest(
    long CancellationRecordId,
    long InvoiceId,
    decimal RequestedAmount,
    string Reason);

public sealed record ApproveRefundRequest(
    decimal ApprovedAmount,
    string? Remarks);
