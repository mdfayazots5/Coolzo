namespace Coolzo.Contracts.Requests.Billing;

public sealed record RecordPaymentRequest(
    long InvoiceId,
    decimal PaidAmount,
    string PaymentMethod,
    string? ReferenceNumber,
    string? Remarks,
    string? IdempotencyKey,
    string? GatewayTransactionId,
    string? Signature,
    decimal? ExpectedInvoiceAmount,
    bool IsWebhookEvent,
    string? WebhookReference);
