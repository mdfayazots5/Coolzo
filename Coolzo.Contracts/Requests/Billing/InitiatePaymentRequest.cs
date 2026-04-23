namespace Coolzo.Contracts.Requests.Billing;

public sealed record InitiatePaymentRequest(
    long InvoiceId,
    string Method);
