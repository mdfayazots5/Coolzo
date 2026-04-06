namespace Coolzo.Contracts.Responses.Billing;

public sealed record PaymentReceiptResponse(
    long PaymentReceiptId,
    string ReceiptNumber,
    long InvoiceId,
    long PaymentTransactionId,
    DateTime ReceiptDateUtc,
    decimal ReceivedAmount,
    decimal BalanceAmount,
    string ReceiptRemarks);
