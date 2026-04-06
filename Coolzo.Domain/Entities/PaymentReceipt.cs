namespace Coolzo.Domain.Entities;

public sealed class PaymentReceipt : AuditableEntity
{
    public long PaymentReceiptId { get; set; }

    public long InvoiceHeaderId { get; set; }

    public long PaymentTransactionId { get; set; }

    public string ReceiptNumber { get; set; } = string.Empty;

    public DateTime ReceiptDateUtc { get; set; }

    public decimal ReceivedAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public string ReceiptRemarks { get; set; } = string.Empty;

    public InvoiceHeader? InvoiceHeader { get; set; }

    public PaymentTransaction? PaymentTransaction { get; set; }
}
