namespace Coolzo.Contracts.Responses.Billing;

public sealed record PaymentGatewayStatusResponse(
    string PaymentId,
    long InvoiceId,
    string Status,
    string? PaymentUrl);
