using Coolzo.Domain.Enums;

namespace Coolzo.Domain.Entities;

public sealed class PaymentTransaction : AuditableEntity
{
    public long PaymentTransactionId { get; set; }

    public long InvoiceHeaderId { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    public string ReferenceNumber { get; set; } = string.Empty;

    public string IdempotencyKey { get; set; } = string.Empty;

    public string GatewayTransactionId { get; set; } = string.Empty;

    public string GatewaySignature { get; set; } = string.Empty;

    public string WebhookReference { get; set; } = string.Empty;

    public decimal PaidAmount { get; set; }

    public DateTime PaymentDateUtc { get; set; }

    public string TransactionRemarks { get; set; } = string.Empty;

    public InvoiceHeader? InvoiceHeader { get; set; }

    public PaymentReceipt? PaymentReceipt { get; set; }

    public ICollection<BillingStatusHistory> BillingStatusHistories { get; set; } = new List<BillingStatusHistory>();
}
