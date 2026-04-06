namespace Coolzo.Contracts.Responses.Billing;

public sealed record PaymentTransactionResponse(
    long PaymentTransactionId,
    long InvoiceId,
    string PaymentMethod,
    string ReferenceNumber,
    decimal PaidAmount,
    DateTime PaymentDateUtc,
    string TransactionRemarks,
    PaymentReceiptResponse? Receipt);
